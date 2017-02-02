using System;
using System.Web;
using System.Web.UI;
using JWS.Fields.Library.Helpers;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.ContentSearch.Linq.Extensions;
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
        private const string LONGITUDE_FIELD_NAME = "LongitudeField";
        private const string LATITIUDE_FIELD_NAME = "LatitudeField";
        private const string APIKEY = "APIKey";
        private const string LABELFIELDNAME = "LabelFieldName";
        private const string COLOR = "Color";

        public GoogleMapField()
        {
            Class = "scContentControl";
            Activation = true;
        }

        private Image _mapImageCtrl;

        private int _mapZoomFactor = DEFAULT_ZOOM;
        private string _mapType = "roadmap";
        private string _mapLabelColor = "Blue";

        private bool Debug { get; set; }

        public override void HandleMessage(Message message)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                var array = Value.Split(',');
                if (array.Length > 0)
                {
                    int.TryParse(array[0], out _mapZoomFactor);
                }

                if (array.Length > 1)
                {
                    _mapType = array[1];
                }
            }

            string messageText;
            if ((messageText = message.Name) == null)
            {
                return;
            }
            if (messageText.Trim() == "contentfile:roadmap")
            {
                _mapType = "roadmap";
                Value = string.Format("{0},{1}", _mapZoomFactor, _mapType);
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }
            if (messageText.Trim() == "contentfile:satellite")
            {
                _mapType = "satellite";
                Value = string.Format("{0},{1}", _mapZoomFactor, _mapType);
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }
            if (messageText.Trim() == "contentfile:hybrid")
            {
                _mapType = "hybrid";
                Value = string.Format("{0},{1}", _mapZoomFactor, _mapType);
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }

            if (messageText.Trim() == "contentfile:terrain")
            {
                _mapType = "terrain";
                Value = string.Format("{0},{1}", _mapZoomFactor, _mapType);
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }

            if (messageText.Trim() == "contentfile:zoom")
            {
                _mapZoomFactor = _mapZoomFactor + 1 > 20 ? _mapZoomFactor : _mapZoomFactor + 1;
                Value = string.Format("{0},{1}", _mapZoomFactor, _mapType);
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }

            if (messageText == "contentfile:zoom-")
            {
                _mapZoomFactor = _mapZoomFactor - 1 < 0 ? _mapZoomFactor : _mapZoomFactor - 1;
                Value = string.Format("{0},{1}", _mapZoomFactor, _mapType);
                Sitecore.Context.ClientPage.Dispatch(CONTENT_EDITOR_SAVE);
                return;
            }

            if (messageText == "contentfile:reset")
            {
                _mapType = "roadmap";
                _mapZoomFactor = _mapZoomFactor - 1 < 0 ? _mapZoomFactor : _mapZoomFactor - 1;
                Value = string.Format("{0},{1}", DEFAULT_ZOOM, _mapType);
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

        protected string LongitudeFieldName { get; private set; }

        protected string LatitiudeFieldName { get; private set; }

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

            LatitiudeFieldName = "Latitude";
            LongitudeFieldName = "Longitude";

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

            if (!string.IsNullOrEmpty(parameters.Parameters[LONGITUDE_FIELD_NAME]))
            {
                LongitudeFieldName = parameters.Parameters[LONGITUDE_FIELD_NAME];
            }

            if (!string.IsNullOrEmpty(parameters.Parameters[LATITIUDE_FIELD_NAME]))
            {
                LatitiudeFieldName = parameters.Parameters[LATITIUDE_FIELD_NAME];
            }

            if (!string.IsNullOrEmpty(parameters.Parameters[APIKEY]))
            {
                APIKey = parameters.Parameters[APIKEY];
            }

            if (!string.IsNullOrEmpty(parameters.Parameters[LABELFIELDNAME]))
            {
                LabelFieldName = parameters.Parameters[LABELFIELDNAME];
            }

            if (!string.IsNullOrEmpty(parameters.Parameters[COLOR]))
            {
                _mapLabelColor = parameters.Parameters[COLOR];
            }

            if (!string.IsNullOrEmpty(parameters.Parameters[ISDEBUG]))
            {
                Debug = MainUtil.GetBool(parameters.Parameters[ISDEBUG], false);
            }
        }

        public string LabelFieldName { get; set; }

        public string APIKey { get; set; }

        protected override void Render(HtmlTextWriter output)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                var array = Value.Split(',');
                if (array.Length > 0)
                {
                    int.TryParse(array[0], out _mapZoomFactor);
                }

                if (array.Length > 1)
                {
                    _mapType = array[1];
                }
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
                    ImageUrl = GetGoogleMapImageUrl(location, centre, item)
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

        private string GetGeoData(BaseItem item, ref string centre)
        {
            double lat;
            double lng;
            double.TryParse(item[LatitiudeFieldName], out lat);
            double.TryParse(item[LongitudeFieldName], out lng);

            var location = string.Format("{0},{1}", lat, lng);

            if (lat == 0.00 && lng == 0.00)
            {
                location = item[AddressFieldName].Replace("<br>", ", ");

                location = HttpUtility.UrlEncode(location);

                if (string.IsNullOrEmpty(location))
                {
                    centre = location;
                }
            }
            return location;
        }

        private string GetGoogleMapImageUrl(string location, string centre, Item item)
        {
            var label = !string.IsNullOrEmpty(LabelFieldName) && !string.IsNullOrEmpty(item[LabelFieldName]) ? item[LabelFieldName].Left(1) : "1";
            return
                string.Format(
                    !string.IsNullOrEmpty(location)
                        ? "http://maps.googleapis.com/maps/api/staticmap?q={0}&zoom={1}&size={2}x{3}&sensor=false&maptype={7}&markers=color:{8}%7Clabel:{6}%7C{0}&key={5}"
                        : "http://maps.googleapis.com/maps/api/staticmap?center={4}&zoom={1}&size={2}x{3}&sensor=false&maptype={7}&markers=color:{8}%7Clabel:{6}%7C{0}&key={5}",
                    location, _mapZoomFactor, MAP_WIDTH, MAP_HEIGHT, centre, APIKey, label, _mapType, _mapLabelColor.ToLower());
        }

        #endregion
    }
}
