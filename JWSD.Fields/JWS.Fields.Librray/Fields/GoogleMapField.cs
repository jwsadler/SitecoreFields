using System;
using System.Web;
using System.Web.UI;
using JWS.Fields.Library.Helpers;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Control = Sitecore.Web.UI.HtmlControls.Control;
using Image = System.Web.UI.WebControls.Image;


namespace JWS.Fields.Library.Fields
{
    public class GoogleMapField : Control, IContentField
    {
        private const int MAP_HEIGHT = 300;
        private const int MAP_WIDTH = 600;
        private const string CONTENT_EDITOR_SAVE = "contenteditor:save";
        private const int DEFAULT_ZOOM = 14;
        private const string OVERRIDEZOOM = "OverrideZoom";
        private const string ISDEBUG = "Debug";
        private const string ADDRESSFIELDNAME = "AddressField";

        private Image _mapImageCtrl;

        private int _mapZoomFactor = DEFAULT_ZOOM;
        private bool Debug { get; set; }

        public override void HandleMessage(Message message)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                int.TryParse(Value, out _mapZoomFactor);
            }

            string messageText;
            if ((messageText = message.Name) == null)
            {
                return;
            }

            if (messageText.Trim() == "contentfile:zoom")
            {
                _mapZoomFactor = _mapZoomFactor + 1 > 20 ? _mapZoomFactor : _mapZoomFactor + 1;
                Value = _mapZoomFactor.ToString();
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }

            if (messageText == "contentfile:zoom-")
            {
                _mapZoomFactor = _mapZoomFactor - 1 < 0 ? _mapZoomFactor : _mapZoomFactor - 1;
                Value = _mapZoomFactor.ToString();
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }

            if (messageText == "contentfile:reset")
            {
                _mapZoomFactor = _mapZoomFactor - 1 < 0 ? _mapZoomFactor : _mapZoomFactor - 1;
                Value = DEFAULT_ZOOM.ToString();
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }
            base.HandleMessage(message);
        }

        #region Implementation of IContentField

        public string Source { get; set; }
        public string ItemID { get; set; }
        protected int OverrideZoom { get; private set; }
        protected string AddressFieldName { get; private set; }

        public string GetValue()
        {
            return Value;
        }

        public void SetValue(string value)
        {
            Value = value;
        }

        #endregion

        #region Override

        private void ParseParameters(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return;
            }


            var parameters = new UrlString(source);
            if (!string.IsNullOrEmpty(parameters.Parameters[OVERRIDEZOOM]))
            {
                int zoomValue;
                int.TryParse(parameters.Parameters[OVERRIDEZOOM], out zoomValue);

                if (zoomValue < 0 || zoomValue > 20) zoomValue = DEFAULT_ZOOM;

                OverrideZoom = zoomValue;
            }

            if (!string.IsNullOrEmpty(parameters.Parameters[ADDRESSFIELDNAME]))
            {
                AddressFieldName = parameters.Parameters[ADDRESSFIELDNAME];
            }

            if (!string.IsNullOrEmpty(parameters.Parameters[ISDEBUG]))
            {
                Debug = MainUtil.GetBool(parameters.Parameters[ISDEBUG], false);
            }
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                int.TryParse(Value, out _mapZoomFactor);
            }

            ParseParameters(Source);
            if (!string.IsNullOrEmpty(AddressFieldName))
            {
                base.Render(output);

                var db = Factory.GetDatabase("master") ?? Factory.GetDatabase("web");

                var id = !string.IsNullOrEmpty(ItemID)
                    ? ItemID
                    : ControlAttributes.Substring(ControlAttributes.IndexOf("//master/", StringComparison.Ordinal) + 9,
                        38);
                var item = new ID(id).ToSitecoreItem(db);

                //render other control

                var centre = "";

                //get lat lng

                var location = GetGeoData(item, ref centre);


                _mapImageCtrl = new Image
                {
                    ID = ID + "_Img_MapView",
                    CssClass = "imageMapView",
                    Width = MAP_WIDTH,
                    Height = MAP_HEIGHT,
                    ImageUrl = GetGoogleMapImageUrl(location, centre)
                };
                _mapImageCtrl.Style.Add("padding-top", "5px");

                if (Debug)
                {
                    output.Write(
                        "<div style='background-color:silver;color:white'>{0}</div>",
                        _mapImageCtrl.ImageUrl);
                }

                _mapImageCtrl.RenderControl(output);
            }
        }

        private string GetGeoData(Item item, ref string centre)
        {
            var location = "";

            location = item[AddressFieldName].Replace("<br>", ", ");

            location = HttpUtility.UrlEncode(location);

            if (string.IsNullOrEmpty(location))
            {
                centre = location;
            }
            return location;
        }
        
        private string GetGoogleMapImageUrl(string location, string centre)
        {
            return
                string.Format(
                    !string.IsNullOrEmpty(location)
                        ? "http://maps.googleapis.com/maps/api/staticmap?&q={0}&zoom={1}&size={2}x{3}&sensor=false&maptype=roadmap&markers=color:blue%7Clabel:1%7C{0}"
                        : "http://maps.googleapis.com/maps/api/staticmap?center={4}&zoom={1}&size={2}x{3}&sensor=false&maptype=roadmap&markers=color:blue%7Clabel:1%7C{0}",
                    location, _mapZoomFactor, MAP_WIDTH, MAP_HEIGHT, centre);
        }

        #endregion
    }
}
