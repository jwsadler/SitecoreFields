using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Checkbox = Sitecore.Web.UI.HtmlControls.Checkbox;

namespace JWS.Fields.Library.Classes
{
    public static class HelperControls
    {
        public static T CreateSitecoreControl<T>(string name, string value, ControlCollection controls,
            string idFromName, bool trackModified, bool disabled)
            where T : Input, new()
        {
            return CreateSitecoreControl<T>(name, true, false, true, value, controls, idFromName, trackModified,
                disabled);
        }

        public static T CreateSitecoreControl<T>(string name, string value, bool readOnly,
            ControlCollection controls, string idFromName, bool trackModified,
            bool disabled) where T : Input, new()
        {
            return CreateSitecoreControl<T>(name, true, readOnly, true, value, controls, idFromName, trackModified,
                disabled);
        }

        public static T CreateSitecoreControl<T>(string name, string value, Unit width, ControlCollection controls,
            string idFromName, bool trackModified, bool disabled)
            where T : Input, new()
        {
            var control = CreateSitecoreControl<T>(name, value, controls, idFromName, trackModified, disabled);
            control.Width = width;
            return control;
        }

        public static T CreateSitecoreControl<T>(string name, string value, Unit width, bool readOnly,
            ControlCollection controls, string idFromName, bool trackModified,
            bool disabled) where T : Input, new()
        {
            var control = CreateSitecoreControl<T>(name, value, readOnly, controls, idFromName, trackModified,
                disabled);
            control.Width = width;
            return control;
        }

        public static T CreateSitecoreControl<T>(string name, bool visible, bool readOnly, bool add, string value,
            ControlCollection controls, string idFromName, bool trackModified,
            bool disabled)
            where T : Input, new()
        {
            var control = new T
            {
                ID = idFromName,
                TrackModified = trackModified,
                Disabled = disabled,
                Visible = visible,
                ReadOnly = readOnly,
                Value = value,
                EnableViewState = true
            };
            if (add) controls.Add(control);
            control.Attributes.Add("style", readOnly ? "background-color: #D3D3D3;" : "");
            if (control.GetType() == typeof (Number) || control.GetType() == typeof (Integer))
            {
                control.Attributes.Add("style", control.Attributes["style"] + "text-align: right;");
            }
            return control;
        }

        public static Checkbox CreateSitecoreControl(string name, bool check, string text,
            ControlCollection controls, string idFromName,
            bool trackModified, bool disabled)
        {
            return CreateSitecoreControl(name, true, false, true, check, text, controls, idFromName, trackModified,
                disabled);
        }

        public static Checkbox CreateSitecoreControl(string name, bool visible, bool readOnly, bool add, bool check,
            string text, ControlCollection controls, string idFromName,
            bool trackModified, bool disabled)
        {
            var control = new Checkbox
            {
                ID = idFromName,
                TrackModified = trackModified,
                Disabled = disabled,
                Visible = visible,
                ReadOnly = readOnly,
                EnableViewState = true
            };
            if (add) controls.Add(control);
            if (!string.IsNullOrEmpty(text))
            {
                CreateLitertal(string.Format("&nbsp;<label for='{0}'>{1}</label>", idFromName, text), controls);
            }

            control.Checked = check;
            return control;
        }

        public static void CreateLitertal(string text, ControlCollection controls)
        {
            controls.Add(new LiteralControl(text));
        }

        public static T CreateControl<T>(string name, bool visible, bool readOnly, bool add, Unit width,
            ControlCollection controls, string idFromName, bool disabled) where T : WebControl, new()
        {
            var control = new T
            {
                ID = idFromName,
                Enabled = !disabled,
                Visible = visible,
                EnableViewState = true,
                Width = width
            };
            if (add) controls.Add(control);
            control.Attributes.Add("style", readOnly ? "background-color: #D3D3D3;" : "");
            return control;
        }
    }
}