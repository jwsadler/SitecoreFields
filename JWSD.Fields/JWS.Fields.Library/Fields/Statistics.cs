using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JWS.Fields.Library.Classes;
using JWS.Fields.Library.Helpers;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Constants = JWS.Fields.Library.Classes.Constants;
using DateTime = System.DateTime;

namespace JWS.Fields.Library.Fields
{
    public class Statistics : Control
    {
        private const string TARGETDATABASE = "Target Database";
        private string _itemId = string.Empty;

        public Statistics()
        {
            Setup();
        }

        private void Setup()
        {
            Class = "scContentControl";
            Activation = true;
        }

        public string ItemId
        {
            get { return _itemId; }
            set { _itemId = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                CreateControls();
            }
            base.OnLoad(e);
        }

        public string Render()
        {
            CreateControls();

            return RenderAsText();
        }

        protected void CreateControls()
        {
            Controls.Clear();
            Item item = null;
            var published = new List<Item>();

            Item[] targets = null;

            try
            {
                var db = Factory.GetDatabase(Constants.Databases.MASTERDB) ??
                         Factory.GetDatabase(Constants.Databases.WEBDB);

                var id = !string.IsNullOrEmpty(ItemId)
                    ? ItemId
                    : ControlAttributes.Substring(ControlAttributes.IndexOf("//master/", StringComparison.Ordinal) + 9,
                        38);
                item = new ID(id).ToSitecoreItem(db);

                var targetsRoot = item.Database.GetItem(
                    Constants.Sitecore.PUBLISHING_TARGETS);

                targets = targetsRoot.GetChildren().ToArray();


                foreach (var target in targets)
                {
                    try
                    {
                        var webItem = item.ID.ToSitecoreItem(Factory.GetDatabase(target[TARGETDATABASE]));
                        if (webItem != null && webItem.Versions.Count != 0)
                        {
                            HelperControls.CreateLitertal("Published Version on " + target[TARGETDATABASE], Controls);
                            HelperControls.CreateSitecoreControl<Integer>("version" + target[TARGETDATABASE], true,
                                true, true,
                                webItem.Version.ToString(), Controls,
                                GetID("version" + target.Name), false, false);

                            published.Add(webItem);
                        }
                        else if (webItem != null && webItem.Versions.Count == 0)
                        {
                            HelperControls.CreateLitertal("Published, but no Versions on " + target[TARGETDATABASE],
                                Controls);
                        }
                        else
                        {
                            HelperControls.CreateLitertal("Not published on " + target[TARGETDATABASE], Controls);
                        }
                    }
                    catch (Exception)
                    {
                        HelperControls.CreateLitertal("Target DB " + target[TARGETDATABASE] + " not found.", Controls);
                    }
                    finally
                    {
                        HelperControls.CreateLitertal("<br />", Controls);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, this);
            }
            finally
            {
                if (item != null)
                {
                    HelperControls.CreateLitertal("<br />", Controls);
                    HelperControls.CreateLitertal("Number of Versions on master", Controls);
                    HelperControls.CreateSitecoreControl<Integer>("version_master", true, true, true,
                        item.Versions.Count.ToString(), Controls,
                        GetID("version_master"), false, false);
                    HelperControls.CreateLitertal("<br />", Controls);
                }

                try
                {
                    HelperControls.CreateLitertal("<br />", Controls);
                    HelperControls.CreateLitertal(
                        "<table width='100%'><tr><td colspan=7 style='background-color:silver;color:white'><center>Version Details</center></td><td >&nbsp;&nbsp;</td><td style='background-color:silver;color:white'></td></tr><tr><td style='background-color:silver;color:white'><center>Version</center></td><td style='background-color:silver;color:white'><center>Created By</center></td><td style='background-color:silver;color:white'><center>Created Date</center></td><td style='background-color:silver;color:white'><center>Modified By</center></td><td style='background-color:silver;color:white'><center>Modified Date</center></td><td style='background-color:silver;color:white'><center>Languages</center></td><td style='background-color:silver;color:white'><center>Work-Flow State</center></td><td><center>&nbsp;&nbsp;</center></td><td style='background-color:silver;color:white'><center>Published</center></td></tr><tbody>",
                        Controls);

                    if (item != null)
                    {
                        var versions =
                            (from p in item.Versions.GetVersions(true).ToArray() orderby p.Version.Number select p).ToList();

                        foreach (var version in versions)
                        {
                            DateTime outValCreate;
                            DateTime outValModify;
                            string workFlowName;
                            GetOutValCreate(version, out outValCreate, out outValModify, out workFlowName);

                            var isPublished = false;

                            var pub =
                                (published.Where(
                                    p =>
                                        p.Version.Number == version.Version.Number &&
                                        p.Languages.Where(
                                            l =>
                                                p.Database.GetItem(p.ID, l) != null &&
                                                p.Database.GetItem(p.ID, l).Versions.Count > 0)
                                            .Select(l => l.Name)
                                            .Contains(version.Language.Name))).
                                    FirstOrDefault();

                            if (pub != null)
                            {
                                isPublished = true;
                            }

                            HelperControls.CreateLitertal(
                                "<tr><td style='" +
                                (isPublished ? "background-color:lightgreen;color:black;" : string.Empty) +
                                "'><center>" + version.Version.Number + "</center></td><td" +
                                (isPublished ? " style='background-color:lightgreen;color:black;'" : string.Empty) +
                                "><center>" +
                                version.Fields[FieldIDs.CreatedBy].Value + "</center></td><td" +
                                (isPublished ? " style='background-color:lightgreen;color:black;'" : string.Empty) +
                                "><center>" +
                                (outValCreate == DateTime.MinValue
                                    ? string.Empty
                                    : outValCreate.ToString("dd MMM yyyy HH:mm")) + "</center></td><td" +
                                (isPublished ? " style='background-color:lightgreen;color:black;'" : string.Empty) +
                                "><center>" +
                                version.Fields[FieldIDs.UpdatedBy].Value + "</center></td><td" +
                                (isPublished ? " style='background-color:lightgreen;color:black;'" : string.Empty) +
                                "><center>" +
                                (outValModify == DateTime.MinValue
                                    ? string.Empty
                                    : outValModify.ToString("dd MMM yyyy HH:mm")) + "</td><td" +
                                (isPublished ? " style='background-color:lightgreen;color:black;'" : string.Empty) +
                                "><center>" + version.Language.Name+ "</center>" +
                                "</td><td" +
                                (isPublished ? " style='background-color:lightgreen;color:black;'" : string.Empty) +
                                "><center>" +
                                workFlowName + "</center></td>" +
                                "</td><td><center></center></td>" +
                                "</td><td><center>" + (isPublished ? "◄" : string.Empty) + "</center></td>"
                                + "</tr>",
                                Controls);
                        }
                    }

                    HelperControls.CreateLitertal("</tbody></table>", Controls);

                    if (targets != null)
                    {
                        foreach (var target in targets)
                        {
                            try
                            {
                                var errMessage = string.Empty;
                                Item webItem;
                                try
                                {
                                    webItem = item.ID.ToSitecoreItem(Factory.GetDatabase(target[TARGETDATABASE]));
                                }
                                catch (Exception ex)
                                {
                                    errMessage = ex.Message;
                                    webItem = null;
                                }
                                if (webItem != null)
                                {
                                    DateTime outValCreate;
                                    DateTime outValModify;
                                    string workFlowName;
                                    GetOutValCreate(webItem, out outValCreate, out outValModify, out workFlowName);

                                    HelperControls.CreateLitertal("<br />", Controls);
                                    HelperControls.CreateLitertal(
                                        "<table width='100%'><tr><td colspan=7 style='background-color:silver;color:white'><center>Item Details on " +
                                        target[TARGETDATABASE] +
                                        "</center></td></tr><tr><td style='background-color:silver;color:white'><center>Version</center></td><td style='background-color:silver;color:white'><center>Created By</center></td><td style='background-color:silver;color:white'><center>Created Date</center></td><td style='background-color:silver;color:white'><center>Modified By</center></td><td style='background-color:silver;color:white'><center>Modified Date</center></td><td style='background-color:silver;color:white'><center>Languages</center></td><td style='background-color:silver;color:white'><center>Work-Flow State</center></td></tr><tbody>",
                                        Controls);

                                    HelperControls.CreateLitertal(
                                        "<tr><td><center>" + webItem.Version.Number +
                                        "</center></td><td><center>" +
                                        webItem.Fields[FieldIDs.CreatedBy].Value + "</center></td><td><center>" +
                                        (outValCreate == DateTime.MinValue
                                            ? string.Empty
                                            : outValCreate.ToString("dd MMM yyyy HH:mm")) + "</center></td><td><center>" +
                                        webItem.Fields[FieldIDs.UpdatedBy].Value + "</td><td><center>" +
                                        (outValModify == DateTime.MinValue
                                            ? string.Empty
                                            : outValModify.ToString("dd MMM yyyy HH:mm")) + "</center>" +
                                        "</td><td><center>" +
                                        string.Join(",", webItem.Languages.Where(p => webItem.Database.GetItem(webItem.ID, p) != null && webItem.Database.GetItem(webItem.ID, p).Versions.Count > 0).Select(p => p.Name).ToArray())
                                        + "</center></td><td><center>" +
                                        workFlowName + "</center></td></tr>",
                                        Controls);
                                }
                                else
                                {
                                    HelperControls.CreateLitertal("<br />", Controls);
                                    HelperControls.CreateLitertal(
                                        "<table width='100%'><tr><td style='background-color:silver;color:white'><center>Item Details on " +
                                        target[TARGETDATABASE] +
                                        "</center></td></tr><tbody>",
                                        Controls);
                                    HelperControls.CreateLitertal(
                                        "<tr><td style='text-align:center;' >No Item Found" +
                                        (!string.IsNullOrEmpty(errMessage)
                                            ? " ( <span style='color:red;'>" + errMessage + "</span>)"
                                            : string.Empty) + "</td></tr>",
                                        Controls);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message, ex, this);
                            }
                            finally
                            {
                                HelperControls.CreateLitertal("</tbody></table>", Controls);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex, this);
                }
            }
        }

        private static void GetOutValCreate(Item version, out DateTime outValCreate, out DateTime outValModify,
            out string workFlowName)
        {
            try
            {
                outValCreate = version.Statistics.Created;
            }
            catch (Exception)
            {
                outValCreate = DateTime.MinValue;
            }

            try
            {
                outValModify = version.Statistics.Updated;
            }
            catch (Exception)
            {
                outValModify = DateTime.MinValue;
            }

            try
            {
                if (!string.IsNullOrEmpty(version.Fields[FieldIDs.WorkflowState].Value))
                {
                    switch (version.Fields[FieldIDs.WorkflowState].Value)
                    {
                        default:
                            workFlowName = "Unknown";
                            break;
                    }
                }
                else
                {
                    workFlowName = "No Work-Flow";
                }
            }
            catch (Exception)
            {
                workFlowName = "Unknown";
            }
        }

        protected void CleanUp(ClientPipelineArgs args)
        {
            if (args.Parameters["initialCall"] == "1")
            {
                args.Parameters["initialCall"] = "0";
                Sitecore.Context.ClientPage.ClientResponse.YesNoCancel(
                    "Are you sure you want to delete all the un-used ETL created versions?",
                    "200", "100");
                args.WaitForPostBack();
            }
        }

        public override void HandleMessage(Message message)
        {
            {
                string messageText;
                if ((messageText = message.Name) == null)
                {
                    return;
                }

                if (messageText == "contentfile:cleanupversions")
                {
                    var nvc = new NameValueCollection {{"initialCall", "1"}};
                    Sitecore.Context.ClientPage.Start(this, "CleanUp", nvc);
                }
            }
        }
    }
}