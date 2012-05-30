using System;
using System.Collections.Generic;
using System.Text;
using  SenseNet.ContentRepository.Schema;
using System.Xml.XPath;
using System.Xml;
using System.Web;

namespace SenseNet.ContentRepository.Fields
{
	public class IntegerFieldSetting : FieldSetting
	{
		public const string MaxValueName = "MaxValue";
		public const string MinValueName = "MinValue";
        public const string ShowAsPercentageName = "ShowAsPercentage";

        private int? _minValue;
		private int? _maxValue;
        private bool? _showAsPercentage;

        private int _slotMinValue;
        private int _slotMaxValue;

		public int? MinValue
		{
			get
			{
                if (_minValue.HasValue)
                    return _minValue.Value;
			    
                return ParentFieldSetting == null ? null : ((IntegerFieldSetting)this.ParentFieldSetting).MinValue;
			} 
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting MinValue is not allowed within readonly instance.");

                if (_slotMinValue == 0)
                    this.Initialize();

                _minValue = value.HasValue ? Math.Max(value.Value, _slotMinValue) : value;
            }
		}
		public int? MaxValue
		{
			get
			{
                if (_maxValue.HasValue)
                    return _maxValue.Value;

                return ParentFieldSetting == null ? null : ((IntegerFieldSetting)this.ParentFieldSetting).MaxValue;
			}
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting MaxValue is not allowed within readonly instance.");

                if (_slotMaxValue == 0)
                    this.Initialize();
                
                _maxValue = value.HasValue ? Math.Min(value.Value, _slotMaxValue) : value;
            }
		}
        public bool? ShowAsPercentage
        {
            get
            {
                if (_showAsPercentage.HasValue)
                    return _showAsPercentage.Value;

                return this.ParentFieldSetting == null ? null :
                    ((IntegerFieldSetting)this.ParentFieldSetting).ShowAsPercentage;
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting ShowAsPercentage is not allowed within readonly instance.");
                _showAsPercentage = value;
            }
        }

		protected override void ParseConfiguration(XPathNavigator configurationElement, IXmlNamespaceResolver xmlNamespaceResolver, ContentType contentType)
		{
            //<MinValue>-6</MinValue>
			//<MaxValue>42</MaxValue>
			foreach (XPathNavigator node in configurationElement.SelectChildren(XPathNodeType.Element))
			{
				switch (node.LocalName)
				{
					case MinValueName:
                        int minValue;
                        if (Int32.TryParse(node.InnerXml, out minValue))
                            _minValue = minValue;
						break;
					case MaxValueName:
						int maxValue;
						if (Int32.TryParse(node.InnerXml, out maxValue))
							_maxValue = maxValue;
						break;
                    case ShowAsPercentageName:
                        bool perc;
                        if (Boolean.TryParse(node.InnerXml, out perc))
                            _showAsPercentage = perc;
                        break;
				}
			}
		}
        public override void Initialize()
        {
            SetSlotMinMaxValues();
        }
		protected override void SetDefaults()
		{
			_minValue = null;
			_maxValue = null;
            _showAsPercentage = null;
		}

		public override FieldValidationResult ValidateData(object value, Field field)
		{
            if ((value == null) && (this.Compulsory ?? false))
                return new FieldValidationResult(CompulsoryName);

            if (value != null)
            {
                if (_slotMinValue == 0)
                    this.Initialize();

                var intValue = (int)value;
                var min = this.MinValue ?? _slotMinValue;
                var max = this.MaxValue ?? _slotMaxValue;

                if (intValue < min)
                {
                    var result = new FieldValidationResult(MinValueName);
                    result.AddParameter(MinValueName, min);
                    return result;
                }

                if (intValue > max)
                {
                    var result = new FieldValidationResult(MaxValueName);
                    result.AddParameter(MaxValueName, max);
                    return result;
                }
                
            }
			return FieldValidationResult.Successful;
		}

        protected override void CopyPropertiesFrom(FieldSetting source)
        {
            base.CopyPropertiesFrom(source);
            var fsSource = (IntegerFieldSetting)source;

            MinValue = fsSource.MinValue;
            MaxValue = fsSource.MaxValue;
            ShowAsPercentage = fsSource.ShowAsPercentage;
        }

	    private void SetSlotMinMaxValues()
        {
	        var propertyType = GetHandlerSlot(0);

	        if (propertyType == typeof(Int32))
            {
                _slotMinValue = Int32.MinValue;
                _slotMaxValue = Int32.MaxValue;
            }
            else if (propertyType == typeof(Byte))
            {
                _slotMinValue = Byte.MinValue;
                _slotMaxValue = Byte.MaxValue;
            }
            else if (propertyType == typeof(Int16))
            {
                _slotMinValue = Int16.MinValue;
                _slotMaxValue = Int16.MaxValue;
            }
            else if (propertyType == typeof(SByte))
            {
                _slotMinValue = SByte.MinValue;
                _slotMaxValue = SByte.MaxValue;
            }
            else if (propertyType == typeof(UInt16))
            {
                _slotMinValue = UInt16.MinValue;
                _slotMaxValue = UInt16.MaxValue;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        //private int GetSlotMinValue()
        //{
        //    var propertyType = GetHandlerSlot(0);
        //    if (propertyType == typeof(Int32))
        //        return Int32.MinValue;
        //    else if (propertyType == typeof(Byte))
        //        return Byte.MinValue;
        //    else if (propertyType == typeof(Int16))
        //        return Int16.MinValue;
        //    else if (propertyType == typeof(SByte))
        //        return SByte.MinValue;
        //    else if (propertyType == typeof(UInt16))
        //        return UInt16.MinValue;
        //    throw new NotImplementedException();
        //}
        //private int GetSlotMaxValue()
        //{
        //    var propertyType = GetHandlerSlot(0);
        //    if (propertyType == typeof(Int32))
        //        return Int32.MaxValue;
        //    else if (propertyType == typeof(Byte))
        //        return Byte.MaxValue;
        //    else if (propertyType == typeof(Int16))
        //        return Int16.MaxValue;
        //    else if (propertyType == typeof(SByte))
        //        return SByte.MaxValue;
        //    else if (propertyType == typeof(UInt16))
        //        return UInt16.MaxValue;
        //    throw new NotImplementedException();
        //}

        protected override void WriteConfiguration(XmlWriter writer)
        {
            WriteElement(writer, this._minValue, MinValueName);
            WriteElement(writer, this._maxValue, MaxValueName);
            WriteElement(writer, this._showAsPercentage, ShowAsPercentageName);
        }

        public override IDictionary<string, FieldMetadata> GetFieldMetadata()
        {
            var fmd = base.GetFieldMetadata();

            var minFs = new IntegerFieldSetting
                            {
                                Name = MinValueName,
                                DisplayName = GetTitleString(MinValueName),
                                Description = GetDescString(MinValueName),
                                ShortName = "Integer",
                                FieldClassName = typeof (IntegerField).FullName
                            };
            var maxFs = new IntegerFieldSetting
                            {
                                Name = MaxValueName,
                                DisplayName = GetTitleString(MaxValueName),
                                Description = GetDescString(MaxValueName),
                                ShortName = "Integer",
                                FieldClassName = typeof (IntegerField).FullName
                            };

            minFs.Initialize();
            maxFs.Initialize();

            fmd.Add(MinValueName, new FieldMetadata
            {
                FieldName = MinValueName, CanRead = true, CanWrite = true, FieldSetting = minFs
            });

            fmd.Add(MaxValueName, new FieldMetadata
            {
                FieldName = MaxValueName, CanRead = true, CanWrite = true, FieldSetting = maxFs
            });

            fmd.Add(ShowAsPercentageName, new FieldMetadata
            {
                FieldName = ShowAsPercentageName, CanRead = true, CanWrite = true,
                FieldSetting = new YesNoFieldSetting
                {
                    Name = ShowAsPercentageName,
                    DisplayName = GetTitleString(ShowAsPercentageName),
                    Description = GetDescString(ShowAsPercentageName),
                    DisplayChoice = DisplayChoice.RadioButtons,
                    DefaultValue = YesNoFieldSetting.NoValue,
                    FieldClassName = typeof(YesNoField).FullName,
                }
            });

            return fmd;
        }

        public override object GetProperty(string name, out bool found)
        {
            var val = base.GetProperty(name, out found);

            if (!found)
            {
                switch (name)
                {
                    case ShowAsPercentageName:
                        found = true;
                        if (_showAsPercentage.HasValue)
                            val = _showAsPercentage.Value ? YesNoFieldSetting.YesValue : YesNoFieldSetting.NoValue;
                        break;
                }
            }

            return found ? val : null;
        }

        public override bool SetProperty(string name, object value)
        {
            var found = base.SetProperty(name, value);

            if (!found)
            {
                switch (name)
                {
                    case ShowAsPercentageName:
                        found = true;
                        if (value != null)
                            _showAsPercentage = YesNoFieldSetting.YesValue.CompareTo(value as string) == 0;
                        break;
                }
            }

            return found;
        }

        protected override SenseNet.Search.Indexing.FieldIndexHandler CreateDefaultIndexFieldHandler()
        {
            return new SenseNet.Search.Indexing.IntegerIndexHandler();
        }
    }
}