﻿//
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

namespace Standard.Licensing;

/// <summary>
/// Implementation of the <see cref="ILicenseBuilder"/>, a fluent api
/// to create new licenses.
/// </summary>
internal class LicenseBuilder : ILicenseBuilder
{
    private readonly Dictionary<string, string> productFeatures = new Dictionary<string, string>();
    private readonly Dictionary<string, string> additionalAttributes = new Dictionary<string, string>();
    private readonly List<License> sublicenses = new();
        
    public Guid Id { get; private set; }

    public IReadOnlyDictionary<string, string> ProductFeatures => productFeatures;

    public Customer Customer { get; private set; }

    public IReadOnlyDictionary<string, string> AdditionalAttributes => additionalAttributes;

    public IReadOnlyList<License> Sublicenses => sublicenses;

    public DateTimeOffset? Expiration { get; private set; }
    
    public LicenseType? Type { get; private set; }
    
    public int Quantity { get; private set; }
        
    public string Signature { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseBuilder"/> class.
    /// </summary>
    public LicenseBuilder()
    {
    }
        
    /// <summary>
    /// Sets the unique identifier of the <see cref="License"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithUniqueIdentifier(Guid id)
    {
        Id = id;
        return this;
    }

    /// <summary>
    /// Sets the expiration date of the <see cref="License"/>.
    /// </summary>
    /// <param name="date">The expiration date of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder ExpiresAt(DateTimeOffset date)
    {
        Expiration = date;
        return this;
    }

    public ILicenseBuilder As(LicenseType type)
    {
        this.Type = type;
        return this;
    }

    public ILicenseBuilder WithMaximumUtilization(int utilization)
    {
        Quantity = utilization;
        return this;
    }

    public ILicenseBuilder LicensedTo(string name)
    {
        Customer = new Customer()
        {
            Name = name
        };
        return this;
    }

    /// <summary>
    /// Sets the <see cref="Customer">license holder</see> of the <see cref="License"/>.
    /// </summary>
    /// <param name="name">The name of the license holder.</param>
    /// <param name="email">The email of the license holder.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder LicensedTo(string name, string email)
    {
        Customer = new Customer()
        {
            Email = email,
            Name = name
        };
        return this;
    }

    /// <summary>
    /// Sets the licensed product features of the <see cref="License"/>.
    /// </summary>
    /// <param name="productFeatures">The licensed product features of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithProductFeatures(IDictionary<string, string> productFeatures)
    {
        this.productFeatures.Clear();
        foreach (var kvp in productFeatures)
        {
            this.productFeatures[kvp.Key] = kvp.Value;
        }
        return this;
    }

    public ILicenseBuilder AddProductFeatures(string featureName, string value)
    {
        this.productFeatures[featureName] = value;
        return this;
    }

    /// <summary>
    /// Sets the licensed additional attributes of the <see cref="License"/>.
    /// </summary>
    /// <param name="additionalAttributes">The additional attributes of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    public ILicenseBuilder WithAdditionalAttributes(IDictionary<string, string> additionalAttributes)
    {
        this.productFeatures.Clear();
        foreach (var kvp in additionalAttributes)
        {
            this.additionalAttributes[kvp.Key] = kvp.Value;
        }
        return this;
    }

    public ILicenseBuilder AddAdditionalAttribute(string attributeName, string value)
    {
        this.additionalAttributes[attributeName] = value;
        return this;
    }

    public ILicenseBuilder WithSublicenses(IEnumerable<License> sublicenses)
    {
        this.sublicenses.Clear();
        foreach (var license in sublicenses)
        {
            this.sublicenses.Add(license);
        }
        return this;
    }

    public ILicenseBuilder AddSublicense(License sublicense)
    {
        this.sublicenses.Add(sublicense);
        return this;
    }

    /// <summary>
    /// Create and sign a new <see cref="License"/> with the specified
    /// private encryption key.
    /// </summary>
    /// <param name="privateKey">The private encryption key for the signature.</param>
    /// <param name="passPhrase">The pass phrase to decrypt the private key.</param>
    /// <returns>The signed <see cref="License"/>.</returns>
    public License CreateAndSignWithPrivateKey(string privateKey, string passPhrase)
    {
        var license = Create();
        return license.Sign(privateKey, passPhrase);
    }

    public License Create()
    {
        var license = new License()
        {
            Id = Id,
            Customer = EqualityComparer<Customer>.Default.Equals(Customer, default) ? null : Customer,
            AdditionalAttributes = AdditionalAttributes.Any() ? AdditionalAttributes : null,
            ProductFeatures = ProductFeatures.Any() ? ProductFeatures : null,
            Expiration = Expiration ?? License.DefaultExpirationDate,
            Quantity = Quantity,
            Sublicenses = sublicenses,
            Type = Type ?? LicenseType.None,
        };
        return license;
    }
}