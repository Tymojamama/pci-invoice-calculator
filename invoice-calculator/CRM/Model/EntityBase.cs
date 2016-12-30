using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using System.Xml.Linq;

namespace InvoiceCalculation.CRM.Model
{
    [Serializable]
    public abstract class EntityBase : DynamicEntity
    {
        public EntityBase(DynamicEntity e)
        {
            this.Name = e.Name;
            this.Properties = new PropertyCollection();
            if (e.Properties != null)
                foreach (var p in e.Properties)
                    this.Properties.Add(p);
        }
        public EntityBase(String name)
            : base(name)
        {
            this.Name = name;
            if (this.Properties == null)
                this.Properties = new PropertyCollection();
        }
        public EntityBase(string name, XElement el)
        {
            this.Name = name;
            this.Attributes = new Dictionary<string, string>();
            foreach (XElement attr in el.Descendants())
                Attributes.Add(attr.Name.LocalName, attr.Value);
        }

        public IDictionary<string, string> Attributes { get; set; }

        protected T GetPropertyValue<T>(string attribute, PropertyType pt, T def)
        {
            try
            {
                switch (pt)
                {
                    case PropertyType.Key:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((Key)this.Properties[attribute]).Value;
                        else
                            return (T)(object)Guid.Parse(this.Attributes[attribute]);
                    case PropertyType.UniqueIdentifier:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((UniqueIdentifier)this.Properties[attribute]).Value;
                        else
                            return (T)(object)Guid.Parse(this.Attributes[attribute]);
                    case PropertyType.Number:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((CrmNumber)this.Properties[attribute]).Value;
                        else
                            return (T)(object)int.Parse(this.Attributes[attribute]);
                    case PropertyType.Money:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((CrmMoney)this.Properties[attribute]).Value;
                        else
                            return (T)(object)int.Parse(this.Attributes[attribute]);
                    case PropertyType.Decimal:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((CrmDecimal)this.Properties[attribute]).Value;
                        else
                            return (T)(object)decimal.Parse(this.Attributes[attribute]);
                    case PropertyType.String:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((string)this.Properties[attribute]);
                        else
                            return (T)(object)this.Attributes[attribute];
                    case PropertyType.DateTime:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((CrmDateTime)this.Properties[attribute]).UniversalTime;
                        else
                            return (T)(object)DateTime.Parse(this.Attributes[attribute]);
                    case PropertyType.Picklist:
                        if (typeof(T) == typeof(Picklist))
                        {
                            return (T)(object)((Picklist)this.Properties[attribute]);
                        }
                        else
                        {
                            if (this.Properties.Contains(attribute))
                                return (T)(object)((Picklist)this.Properties[attribute]).Value;
                            else
                                return (T)(object)int.Parse(this.Attributes[attribute]);
                        }
                    case PropertyType.Owner:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((Owner)this.Properties[attribute]).Value;
                        else
                            return (T)(object)Guid.Parse(this.Attributes[attribute]);
                    case PropertyType.Lookup:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((Lookup)this.Properties[attribute]).Value;
                        else
                            return (T)(object)Guid.Parse(this.Attributes[attribute]);
                    case PropertyType.Customer:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((CrmReference)this.Properties[attribute]).Value;
                        else
                            return (T)(object)Guid.Parse(this.Attributes[attribute]);
                    case PropertyType.Status:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((Status)this.Properties[attribute]).Value;
                        else
                            return (T)(object)int.Parse(this.Attributes[attribute]);
                    case PropertyType.Bit:
                        if (this.Properties.Contains(attribute))
                            return (T)(object)((CrmBoolean)this.Properties[attribute]).Value;
                        else
                            return (T)(object)bool.Parse(this.Attributes[attribute]);
                    default:
                        return def;
                }
            }
            catch { }
            return def;
        }
        protected void SetPropertyValue<T>(string attribute, PropertyType pt, T value)
        {
            try
            {
                switch (pt)
                {
                    case PropertyType.Key:
                        this.Properties[attribute] = new Key((Guid)(object)value);
                        break;
                    case PropertyType.UniqueIdentifier:
                        this.Properties[attribute] = new UniqueIdentifier((Guid)(object)value);
                        break;
                    case PropertyType.Number:
                        this.Properties[attribute] = new CrmNumber((int)(object)value);
                        break;
                    case PropertyType.Money:
                        this.Properties[attribute] = new CrmMoney((decimal)(object)value);
                        break;
                    case PropertyType.Decimal:
                        this.Properties[attribute] = new CrmDecimal((decimal)(object)value);
                        break;
                    case PropertyType.String:
                        this.Properties[attribute] = value;
                        break;
                    case PropertyType.DateTime:
                        this.Properties[attribute] = new CrmDateTime(((DateTime)(object)value).ToString("yyyy-MM-ddTHH:mm:ss Z"));
                        break;
                    case PropertyType.Picklist:
                        int intValue = (int)(object)value;
                        Picklist picklist = new Picklist();
                        if (intValue == 0)
                        {
                            picklist.IsNull = true;
                            picklist.IsNullSpecified = true;
                        }
                        else
                            picklist.Value = intValue;
                        this.Properties[attribute] = picklist;
                        break;
                    case PropertyType.Owner:
                        this.Properties[attribute] = new Owner("systemuser", (Guid)(object)value);
                        break;
                    case PropertyType.Lookup:
                        Guid guidValue = (Guid)(object)value;
                        Lookup lookup = new Lookup();
                        if (guidValue == Guid.Empty)
                        {
                            lookup.IsNull = true;
                            lookup.IsNullSpecified = true;
                        }
                        else
                            lookup.Value = guidValue;
                        this.Properties[attribute] = lookup;
                        //this.Properties[attribute] = new Lookup("systemuser", (Guid)(object)value);
                        break;
                    case PropertyType.Customer:
                        this.Properties[attribute] = new Customer("account", (Guid)(object)value);
                        break;
                    case PropertyType.Bit:
                        this.Properties[attribute] = new CrmBoolean((Boolean)(object)value);
                        break;
                    case PropertyType.Status:
                        this.Properties[attribute] = new Status((int)(object)value);
                        break;
                }
            }
            catch { }
        }

        internal DynamicEntity GetDynamicEntity()
        {
            DynamicEntity de = new DynamicEntity(this.Name);
            de.Properties = new PropertyCollection();
            if (this.Properties != null)
                foreach (var p in this.Properties)
                    de.Properties.Add(p);

            return de;
        }

        protected enum PropertyType
        {
            Key,
            UniqueIdentifier,
            Number,
            Money,
            Decimal,
            String,
            DateTime,
            Picklist,
            Owner,
            Lookup,
            Customer,
            Status,
            Bit
        }

        protected DateTime? StringDateToNullable(String crmDateString)
        {
            DateTime? dotNetDate = new DateTime?();
            DateTime tempDate;
            if (DateTime.TryParse(crmDateString, out tempDate))
                dotNetDate = tempDate;
            return dotNetDate;
        }

        protected DateTime? DateToNullable(DateTime date)
        {
            DateTime? nullableDate = new DateTime?();
            if (date != DateTime.MinValue) { nullableDate = date; }
            return nullableDate;
        }

        protected DateTime NullableToDate(DateTime? nullableDate)
        {
            if (nullableDate.HasValue)
                return nullableDate.Value;
            else
                return DateTime.MinValue;
        }
    }
}
