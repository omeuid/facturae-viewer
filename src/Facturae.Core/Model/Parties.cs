// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.Xml.Serialization;

namespace FacturaeViewer.Core.Model;

public class Parties
{
    [XmlElement("SellerParty", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Party? SellerParty { get; set; }

    [XmlElement("BuyerParty", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Party? BuyerParty { get; set; }
}

/// <summary>Parte interviniente (emisor/receptor): identificación fiscal, centros y empresa o persona física.</summary>
public class Party
{
    [XmlElement("TaxIdentification", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public TaxIdentification? TaxIdentification { get; set; }

    /// <summary>Identificador de la parte en el sistema de facturación (opcional).</summary>
    [XmlElement("PartyIdentification", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PartyIdentification { get; set; }

    [XmlElement("LegalEntity", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public LegalEntity? LegalEntity { get; set; }

    [XmlElement("Individual", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public Individual? Individual { get; set; }

    /// <summary>Solo presente en el receptor (BuyerParty) para facturas a la Administración.</summary>
    [XmlArray("AdministrativeCentres", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlArrayItem("AdministrativeCentre", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AdministrativeCentre[]? AdministrativeCentres { get; set; }
}

public class LegalEntity
{
    [XmlElement("CorporateName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CorporateName { get; set; }

    [XmlElement("TradeName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? TradeName { get; set; }

    [XmlElement("RegistrationData", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public RegistrationData? RegistrationData { get; set; }

    [XmlElement("AddressInSpain", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AddressInSpain? AddressInSpain { get; set; }

    [XmlElement("OverseasAddress", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public OverseasAddress? OverseasAddress { get; set; }

    [XmlElement("ContactDetails", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ContactDetails? ContactDetails { get; set; }
}

public class RegistrationData
{
    [XmlElement("Book", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Book { get; set; }

    [XmlElement("RegisterOfCompaniesLocation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? RegisterOfCompaniesLocation { get; set; }

    [XmlElement("Sheet", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Sheet { get; set; }

    [XmlElement("Folio", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Folio { get; set; }

    [XmlElement("Section", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Section { get; set; }

    [XmlElement("Volume", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Volume { get; set; }

    [XmlElement("AdditionalRegistrationData", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AdditionalRegistrationData { get; set; }
}

public class Individual
{
    [XmlElement("Name", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Name { get; set; }

    [XmlElement("FirstSurname", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? FirstSurname { get; set; }

    [XmlElement("SecondSurname", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? SecondSurname { get; set; }

    [XmlElement("AddressInSpain", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AddressInSpain? AddressInSpain { get; set; }

    [XmlElement("OverseasAddress", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public OverseasAddress? OverseasAddress { get; set; }

    [XmlElement("ContactDetails", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ContactDetails? ContactDetails { get; set; }
}

public class TaxIdentification
{
    /// <summary>Tipo de persona: "F" física, "J" jurídica, "I" identidad extranjera.</summary>
    [XmlElement("PersonTypeCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PersonTypeCode { get; set; }

    /// <summary>Residencia: "R" residente, "E" extranjero, "U" residente en otro país de la UE.</summary>
    [XmlElement("ResidenceTypeCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ResidenceTypeCode { get; set; }

    [XmlElement("TaxIdentificationNumber", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? TaxIdentificationNumber { get; set; }
}

public class AddressInSpain
{
    [XmlElement("Address", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Address { get; set; }

    [XmlElement("PostCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PostCode { get; set; }

    [XmlElement("Town", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Town { get; set; }

    [XmlElement("Province", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Province { get; set; }

    [XmlElement("CountryCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CountryCode { get; set; }
}

public class OverseasAddress
{
    [XmlElement("Address", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Address { get; set; }

    [XmlElement("PostCodeAndTown", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? PostCodeAndTown { get; set; }

    [XmlElement("Province", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Province { get; set; }

    [XmlElement("CountryCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CountryCode { get; set; }
}

public class ContactDetails
{
    [XmlElement("Telephone", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? Telephone { get; set; }

    [XmlElement("TeleFax", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? TeleFax { get; set; }

    [XmlElement("WebAddress", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? WebAddress { get; set; }

    [XmlElement("ElectronicMail", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ElectronicMail { get; set; }

    [XmlElement("ContactPersons", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? ContactPersons { get; set; }

    [XmlElement("CnoCnae", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CnoCnae { get; set; }

    [XmlElement("INETownCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? INETownCode { get; set; }

    [XmlElement("AdditionalContactDetails", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? AdditionalContactDetails { get; set; }
}

public class AdministrativeCentre
{
    [XmlElement("CentreCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CentreCode { get; set; }

    [XmlElement("RoleTypeCode", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? RoleTypeCode { get; set; }

    [XmlElement("AddressInSpain", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public AddressInSpain? AddressInSpain { get; set; }

    [XmlElement("OverseasAddress", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public OverseasAddress? OverseasAddress { get; set; }

    [XmlElement("CentreDescription", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string? CentreDescription { get; set; }
}