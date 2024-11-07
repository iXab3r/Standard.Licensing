//
// Copyright © 2012 - 2013 Nauck IT KG     http://www.nauck-it.de
//
// Author:
//  Daniel Nauck        <d.nauck(at)nauck-it.de>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Standard.Licensing.Security.Cryptography;

namespace Standard.Licensing;

public sealed record License
{
    public static readonly string DefaultExpirationDateString = DateTimeOffset.MaxValue.ToString("r", CultureInfo.InvariantCulture);
    public static readonly DateTimeOffset DefaultExpirationDate = DateTimeOffset.ParseExact(DefaultExpirationDateString, "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    private static readonly string SignatureAlgorithm = X9ObjectIdentifiers.ECDsaWithSha512.Id;

    /// <summary>
    /// To validate signature we need to analyze exactly the same text which was provided.
    /// That is why we're keeping both the parsed data AND the license xml
    /// </summary>
    private readonly XElement loadedLicenseXmlDataWithoutSignature;

    internal License()
    {
        loadedLicenseXmlDataWithoutSignature = default;
    }

    internal License(string licenseXml)
    {
        var licenseElement = XElement.Parse(licenseXml);
        var id = Guid.Parse(licenseElement.Element("Id")?.Value ?? Guid.Empty.ToString());
        var version = int.Parse(licenseElement.Attribute("version")?.Value ?? "0");
        var type = Enum.Parse<LicenseType>(licenseElement.Element("Type")?.Value ?? LicenseType.None.ToString(), true);
        var quantity = int.Parse(licenseElement.Element("Quantity")?.Value ?? "0");
        var expiration = string.IsNullOrEmpty(licenseElement.Element("Expiration")?.Value)
            ? DefaultExpirationDate
            : DateTimeOffset.ParseExact(licenseElement.Element("Expiration")?.Value, "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

        var productFeatures = licenseElement.Element("ProductFeatures")
            ?.Elements("Feature")
            .ToDictionary(e => e.Attribute("name")?.Value, e => e.Value);

        var additionalAttributes = licenseElement.Element("LicenseAttributes")
            ?.Elements("Attribute")
            .ToDictionary(e => e.Attribute("name")?.Value, e => e.Value);

        var sublicenses = licenseElement.Element("Sublicenses")
            ?.Elements("License")
            .Select(x =>
            {
                var sublicenseXml = x.ToString();
                var sublicense = Load(sublicenseXml);
                return sublicense;
            }).ToArray();

        var customerElement = licenseElement.Element("Customer");
        var customer = customerElement != null
            ? new Customer
            {
                Name = customerElement.Element("Name")?.Value,
                Email = customerElement.Element("Email")?.Value
            }
            : null;

        var signatureElement = licenseElement.Element("Signature");
        var signature = signatureElement?.Value;
        signatureElement?.Remove();

        loadedLicenseXmlDataWithoutSignature = licenseElement;

        Id = id;
        Type = type;
        Quantity = quantity;
        Expiration = expiration;
        ProductFeatures = productFeatures;
        AdditionalAttributes = additionalAttributes;
        Customer = customer;
        Version = version;
        Signature = signature;
        Sublicenses = sublicenses;
    }

    public Guid Id { get; init; }

    public Customer Customer { get; init; }

    public DateTimeOffset Expiration { get; init; }

    public LicenseType Type { get; init; }

    public int Quantity { get; init; }
    
    public int Version { get; init; }

    public IReadOnlyDictionary<string, string> ProductFeatures { get; init; }

    public IReadOnlyDictionary<string, string> AdditionalAttributes { get; init; }
    
    public IReadOnlyList<License> Sublicenses { get; init; }

    public string Signature { get; }

    public static ILicenseBuilder New()
    {
        return new LicenseBuilder();
    }

    public static License Load(string licenseXml)
    {
        return new License(licenseXml);
    }

    public static AsymmetricKeyParameter LoadPrivateKey(string privateKey, string passPhrase)
    {
        return KeyFactory.FromEncryptedPrivateKeyString(privateKey, passPhrase);
    }

    public static AsymmetricKeyParameter LoadPublicKey(string publicKey)
    {
        return KeyFactory.FromPublicKeyString(publicKey);
    }
    
    public override string ToString()
    {
        var xmlData = GetLicenseXmlElementWithoutSignature();
        if (!string.IsNullOrEmpty(Signature))
        {
            xmlData.Add(new XElement("Signature", Signature));
        }

        return xmlData.ToString();
    }

    public License Sign(string privateKey, string passPhrase)
    {
        var newLicenseData = GetLicenseXmlElementWithoutSignature();
        var privKey = LoadPrivateKey(privateKey, passPhrase);

        var documentToSign = Encoding.UTF8.GetBytes(newLicenseData.ToString(SaveOptions.DisableFormatting));
        var signer = SignerUtilities.GetSigner(SignatureAlgorithm);
        signer.Init(true, privKey);
        signer.BlockUpdate(documentToSign, 0, documentToSign.Length);
        
        var signatureBytes = signer.GenerateSignature();
        var signatureBase64 = Convert.ToBase64String(signatureBytes);

        newLicenseData.Add(new XElement("Signature", signatureBase64));

        return new License(newLicenseData.ToString());
    }

    public bool VerifySignature(string publicKey)
    {
        var xmlData = loadedLicenseXmlDataWithoutSignature;
        var signTag = Signature;

        if (signTag == null || xmlData == null)
        {
            return false;
        }

        var pubKey = LoadPublicKey(publicKey);

        var dataToSign = xmlData.ToString(SaveOptions.DisableFormatting);
        var documentToSign = Encoding.UTF8.GetBytes(dataToSign);
        var signer = SignerUtilities.GetSigner(SignatureAlgorithm);
        signer.Init(false, pubKey);
        signer.BlockUpdate(documentToSign, 0, documentToSign.Length);

        var signatureBase64 = Convert.FromBase64String(signTag);
        var isValid = signer.VerifySignature(signatureBase64);
        return isValid;
    }

    private XElement GetLicenseXmlElementWithoutSignature()
    {
        var elements = new List<object>();

        if (!EqualityComparer<Guid>.Default.Equals(Id, default))
        {
            elements.Add(new XElement("Id", Id));
        }

        if (!EqualityComparer<LicenseType>.Default.Equals(Type, default))
        {
            elements.Add(new XElement("Type", Type.ToString()));
        }

        if (!EqualityComparer<int>.Default.Equals(Quantity, default))
        {
            elements.Add(new XElement("Quantity", Quantity.ToString()));
        }

        if (!EqualityComparer<Customer>.Default.Equals(Customer, default))
        {
            var customer = new XElement("Customer");
            if (!string.IsNullOrEmpty(Customer.Name))
            {
                customer.Add(new XElement("Name", Customer.Name));
            }

            if (!string.IsNullOrEmpty(Customer.Email))
            {
                customer.Add(new XElement("Email", Customer.Email));
            }

            elements.Add(customer);
        }

        if (AdditionalAttributes != null && AdditionalAttributes.Any())
        {
            var attributes = new XElement("LicenseAttributes");
            elements.Add(attributes);
            foreach (var kvp in AdditionalAttributes)
            {
                var featureElement = new XElement("Attribute");
                featureElement.SetAttributeValue("name", kvp.Key);
                featureElement.Value = kvp.Value;
                attributes.Add(featureElement);
            }
        }

        if (!EqualityComparer<DateTimeOffset>.Default.Equals(Expiration, default))
        {
            elements.Add(new XElement("Expiration", Expiration.ToString("r", CultureInfo.InvariantCulture)));
        }

        if (ProductFeatures != null && ProductFeatures.Any())
        {
            var features = new XElement("ProductFeatures");
            elements.Add(features);
            foreach (var kvp in ProductFeatures)
            {
                var featureElement = new XElement("Feature");
                featureElement.SetAttributeValue("name", kvp.Key);
                featureElement.Value = kvp.Value;
                features.Add(featureElement);
            }
        }

        if (Sublicenses != null && Sublicenses.Any())
        {
            var sublicenses = new XElement("Sublicenses");
            elements.Add(sublicenses);
            
            foreach (var license in Sublicenses)
            {
                var licenseElement = license.GetLicenseXmlElementWithoutSignature();
                sublicenses.Add(licenseElement);
            }
        }
        
        var xmlData = new XElement("License", elements.ToArray());
        if (Version > 0)
        {
            xmlData.SetAttributeValue("version", Version);
        }
        return xmlData;
    }
}