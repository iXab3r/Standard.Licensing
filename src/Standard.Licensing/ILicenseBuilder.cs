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

namespace Standard.Licensing;

/// <summary>
/// Fluent api to create and sign a new <see cref="License"/>. 
/// </summary>
public interface ILicenseBuilder : IFluentInterface
{
    /// <summary>
    /// Gets unique identifier of the <see cref="License"/>.
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// Gets the licensed product features of the <see cref="License"/>.
    /// </summary>
    IReadOnlyDictionary<string, string> ProductFeatures { get; }

    /// <summary>
    /// Gets <see cref="Customer">license holder</see> of the <see cref="License"/>.
    /// </summary>
    Customer Customer { get; }

    /// <summary>
    /// Gets the licensed additional attributes of the <see cref="License"/>.
    /// </summary>
    IReadOnlyDictionary<string, string> AdditionalAttributes { get; }

    /// <summary>
    /// Gets the licensed submodules of the <see cref="License"/>.
    /// </summary>
    IReadOnlyList<License> Sublicenses { get; }

    /// <summary>
    /// Gets expiration date of the <see cref="License"/>.
    /// </summary>
    DateTimeOffset? Expiration { get; }
    
    /// <summary>
    /// Gets the <see cref="LicenseType"/> of the <see cref="License"/>.
    /// </summary>
    LicenseType? Type { get; }
    
    /// <summary>
    /// Sets the unique identifier of the <see cref="License"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder WithUniqueIdentifier(Guid id);

    /// <summary>
    /// Sets the expiration date of the <see cref="License"/>.
    /// </summary>
    /// <param name="date">The expiration date of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder ExpiresAt(DateTimeOffset date);
    
    /// <summary>
    /// Sets the <see cref="LicenseType"/> of the <see cref="License"/>.
    /// </summary>
    /// <param name="type">The <see cref="LicenseType"/> of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder As(LicenseType type);
    
    ILicenseBuilder WithMaximumUtilization(int utilization);
        
    /// <summary>
    /// Sets the <see cref="Customer">license holder</see> of the <see cref="License"/>.
    /// </summary>
    /// <param name="name">The name of the license holder.</param>
    /// <param name="email">The email of the license holder.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder LicensedTo(string name);

    /// <summary>
    /// Sets the <see cref="Customer">license holder</see> of the <see cref="License"/>.
    /// </summary>
    /// <param name="name">The name of the license holder.</param>
    /// <param name="email">The email of the license holder.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder LicensedTo(string name, string email);

    /// <summary>
    /// Sets the licensed product features of the <see cref="License"/>.
    /// </summary>
    /// <param name="productFeatures">The licensed product features of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder WithProductFeatures(IDictionary<string, string> productFeatures);
    
    /// <summary>
    /// Sets the licensed product features of the <see cref="License"/>.
    /// </summary>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder AddProductFeatures(string featureName, string value);

    /// <summary>
    /// Sets the licensed additional attributes of the <see cref="License"/>.
    /// </summary>
    /// <param name="additionalAttributes">The additional attributes of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder WithAdditionalAttributes(IDictionary<string, string> additionalAttributes);
    
    /// <summary>
    /// Sets the licensed additional attributes of the <see cref="License"/>.
    /// </summary>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder AddAdditionalAttribute(string attributeName, string value);

    /// <summary>
    /// Sets the licensed submodules of the <see cref="License"/>.
    /// </summary>
    /// <param name="sublicenses">The additional sublicenses of the <see cref="License"/>.</param>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder WithSublicenses(IEnumerable<License> sublicenses);
    
    /// <summary>
    /// Sets the licensed submodules of the <see cref="License"/>.
    /// </summary>
    /// <returns>The <see cref="ILicenseBuilder"/>.</returns>
    ILicenseBuilder AddSublicense(License sublicense);

    /// <summary>
    /// Create and sign a new <see cref="License"/> with the specified
    /// private encryption key.
    /// </summary>
    /// <param name="privateKey">The private encryption key for the signature.</param>
    /// <param name="passPhrase">The pass phrase to decrypt the private key.</param>
    /// <returns>The signed <see cref="License"/>.</returns>
    License CreateAndSignWithPrivateKey(string privateKey, string passPhrase);

    License Create();
}