using System;
using System.Web.UI;
using JWS.Fields.Library.Classes;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;

namespace JWS.Fields.Library.Fields
{
    public class EnumValidatedIntegerField : Integer
    {
        private const string CLASSNAME = "ClassName";
        private const string ERRORTYPE = "ErrorType";
        private const string METADATATAG = "Tag";
        private const string DEFAULT_ERROR = "Warning";

        private ErrorTypes _errorType;

        public string Source { get; set; }

        public string ClassName { get; set; }

        public string ErrorType { get; set; }

        public string MetadataTag { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            OnLoad(this);
        }

        protected override void DoRender(HtmlTextWriter output)
        {
            ParseParameters(Source);
        }

        private void ParseParameters(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return;
            }


            var parameters = new UrlString(source);
            ClassName = parameters.Parameters[CLASSNAME];
            ErrorType = string.IsNullOrEmpty(parameters.Parameters[ERRORTYPE])
                ? DEFAULT_ERROR
                : parameters.Parameters[ERRORTYPE];

            if (!string.IsNullOrEmpty(ErrorType))
            {
                if (!Enum.TryParse(ErrorType, out _errorType))
                {
                    _errorType = ErrorTypes.Warning;
                }
            }

            MetadataTag = parameters.Parameters[METADATATAG];

            if (string.IsNullOrEmpty(MetadataTag))
            {
                MetadataTag = Constants.DEFAULT_METADATA;
            }

        }


        private static void OnLoad(EnumValidatedIntegerField field)
        {
            field.ParseParameters(field.Source);
        }


        public static void DoRender(Action baseRender, HtmlTextWriter output, EnumValidatedIntegerField field)
        {
            output.Write(@"<div style=""position:relative"">");

            Type type = null;
            //Validate the Enum
            try
            {
                type = Type.GetType(field.ClassName);
            }
            catch (Exception)
            {
            }

            if (type == null)
            {
                output.Write(@"<span style=""color:red;"">The Enum Type Specified {0} cannot be found.</span>",
                    field.ClassName);
            }
            else
            {
                if (!Enum.IsDefined(type, field.Value))
                {
                    var message =
                        string.Format(@"<span style=""color:red;"">The Enum Value Specified {0} is not valid.</span>",
                            field.Value);
                    output.Write(message);

                    switch (field._errorType)
                    {
                        case ErrorTypes.Error:
                            break;
                        case ErrorTypes.Warning:
                            break;
                    }
                }
                else
                {
                    var enumValue = Enum.Parse(type, field.Value);
                    var description = enumValue.GetFirstValueFromMetaDataAttribute(field.MetadataTag);
                    if (string.IsNullOrEmpty(description))
                    {
                        description = enumValue.ToString();
                    }

                    output.Write(@"<span><em>{0}</em>.</span>", description);
                }
            }
           
            baseRender.Invoke();

            output.Write(@"</div>");
        }

        internal enum ErrorTypes
        {
            Warning,
            Error,
            None
        }
    }
}