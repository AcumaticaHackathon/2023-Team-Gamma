using HackathonRoute.Integration.DAC;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using HackathonRoute.Customization.DAC;
using System.Web.Routing;
using static PX.SM.UPCompany;

namespace HackathonRoute.Integration
{
    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; set; }
    }
    public static class Utils
    {

        public static string Description(this Enum enumValue)
        {
            DescriptionAttribute descriptionAttribute = Attribute.GetCustomAttribute(enumValue.GetType().GetField(enumValue.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (descriptionAttribute != null)
            {
                return descriptionAttribute.Description;
            }

            return enumValue.ToString();
        }
    }
    internal class ProcessRouting : PXGraph<ProcessRouting>
    {
        public PXCancel<ProcessRoutingFilter> Cancel;
        public PXFilter<ProcessRoutingFilter> Filter;
        [PXFilterable]
        public PXFilteredProcessingJoin<SOShipment, ProcessRoutingFilter,
            LeftJoin<SOAddress, On<SOAddress.addressID, Equal<SOShipment.shipAddressID>>,
                LeftJoin<SOContact, On<SOContact.contactID, Equal<SOShipment.shipContactID>>>>,
                Where<SOShipment.shipDate, Equal<Current<AccessInfo.businessDate>>,
                    And2<Where<SOShipment.shipVia,Equal<Current<ProcessRoutingFilter.shipVia>>,Or<Current<ProcessRoutingFilter.shipVia>,IsNull>>,
                        And<Where<SOShipment.siteID,Equal<Current<ProcessRoutingFilter.siteID>>,Or<Current<ProcessRoutingFilter.siteID>,IsNull>>>>>> Shipments;
        public PXSetup<SOSetup> sosetup;

        public ProcessRouting()
        {
            SOSetup record = sosetup.Current;
        }

        //public virtual IEnumerable shipments()
        //{


        //    ProcessRoutingFilter filter = PXCache<ProcessRoutingFilter>.CreateCopy(Filter.Current);

        //    PXSelectBase<SOShipment> cmd = GetSelectCommand(filter);

        //    int startRow = PXView.StartRow;
        //    int totalRows = 0;

        //    foreach (SOShipment res in cmd.View.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
        //    {
        //        SOShipment order = res;
        //        SOShipment cached;

        //        if ((cached = (SOShipment)Shipments.Cache.Locate(order)) != null)
        //        {
        //            order.Selected = cached.Selected;
        //        }

        //        yield return order;
        //    }

        //    PXView.StartRow = 0;

        //    Shipments.Cache.IsDirty = false;
        //}

        //private PXSelectBase<SOShipment,
        //        LeftJoin<SOShipmentAddress, On<SOShipmentAddress.addressID, Equal<SOShipment.shipAddressID>>,
        //        LeftJoin<SOShipmentContact, On<SOShipmentContact.contactID, Equal<SOShipment.shipContactID>>>>> GetSelectCommand(ProcessRoutingFilter filter)
        //{
        //    return new PXSelectJoin<SOShipment,
        //        LeftJoin<SOShipmentAddress,On<SOShipmentAddress.addressID,Equal<SOShipment.shipAddressID>>,
        //        LeftJoin<SOShipmentContact,On<SOShipmentContact.contactID,Equal<SOShipment.shipContactID>>>>,
        //        Where<SOShipment.shipDate,Equal<Current<AccessInfo.businessDate>>>>(this);
        //}
        public virtual void _(Events.FieldDefaulting<ProcessRoutingFilter.departureTime> e)
        {
            if(e.Row is ProcessRoutingFilter row)
            {
                e.NewValue = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 7, 0, 0);
            }
        }
        public virtual void ProcessRoutingFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row is ProcessRoutingFilter row)
            {
                Shipments.SetProcessDelegate(
                delegate (List<SOShipment> list)
                {
                    ProcessShipments(row, list, true);
                }
                );
                Shipments.SetProcessCaption(PX.Objects.IN.Messages.Process);
                Shipments.SetProcessAllCaption(PX.Objects.IN.Messages.ProcessAll);
            }
        }
        public static void ProcessShipments(ProcessRoutingFilter filter, List<SOShipment> list, bool isMassProcess) => ProcessShipments(filter, list, isMassProcess, PXQuickProcess.ActionFlow.NoFlow);

        public static void ProcessShipments(ProcessRoutingFilter filter, List<SOShipment> list, bool isMassProcess, PXQuickProcess.ActionFlow processFlow)
        {
            if (!list.Any())
                return;
            bool failed = false;
            SOShipmentEntry sOShipmentEntry = PXGraph.CreateInstance<SOShipmentEntry>();
            SOSetup soSetup = PXSelect<SOSetup>.Select(sOShipmentEntry);
            if (soSetup != null)
            {
                var c_ApiKey = GetApiKey(soSetup);
                RestClient restClient = new RestClient("https://api.route4me.com/");
                restClient.Timeout = -1;
                var soShipment = list.First();
                var warehouseRecord = INSite.PK.Find(sOShipmentEntry, soShipment.SiteID);
                var alladdresses = "";
                if (warehouseRecord != null)
                {
                    var warehouseAddressRecord = Address.PK.Find(sOShipmentEntry, warehouseRecord.AddressID);
                    alladdresses += $"{warehouseAddressRecord.AddressLine1}, {warehouseAddressRecord.City}, {warehouseAddressRecord.State}, {warehouseAddressRecord.PostalCode}" + $"||";
                    foreach (SOShipment shipment in list)
                    {
                        var shipmentAddressRecord = SOShipmentAddress.PK.Find(sOShipmentEntry, shipment.ShipAddressID);
                        alladdresses += $"{shipmentAddressRecord.AddressLine1}, {shipmentAddressRecord.City}, {shipmentAddressRecord.State}, {shipmentAddressRecord.PostalCode}" + $" [{shipment.ShipmentNbr}]||";
                    }
                }
                var getLongLatt = new RestRequest("api/geocoder.php", Method.POST);
                getLongLatt.AddQueryParameter("api_key", c_ApiKey);
                getLongLatt.AddQueryParameter("addresses", alladdresses);
                getLongLatt.AddQueryParameter("format", "json");
                getLongLatt.AddHeader("Content-Type", "application/json");
                IRestResponse responseWithCoords = restClient.Execute(getLongLatt);
                if (responseWithCoords.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var parsedAddresses = JsonConvert.DeserializeObject<List<Route4MeAddress>>(responseWithCoords.Content);
                    if (parsedAddresses != null && parsedAddresses.Count > 0)
                    {
                        parsedAddresses[0].IsDepot = true;
                        parsedAddresses[0].Original = String.Empty;
                        parsedAddresses[0].SequenceNo = 0;
                        parsedAddresses[0].Alias = "WAREHOUSE";
                        for (int i = 1; i < parsedAddresses.Count(); i++)
                        {
                            parsedAddresses[i].Alias = parsedAddresses[i].Original.Substring(parsedAddresses[i].Original.IndexOf('[') + 1, parsedAddresses[i].Original.IndexOf(']') - parsedAddresses[i].Original.IndexOf('[') - 1);
                            parsedAddresses[i].Original = String.Empty;
                            parsedAddresses[i].SequenceNo = i;
                        }
                        Route4MeParameters parameters = new Route4MeParameters()
                        {
                            AlgorithmType = AlgorithmType.TSP,
                            RouteName = "Daily Route from Acumatica",
                            RouteDate = ConvertToUnixTimestamp(DateTime.UtcNow.Date),
                            RouteTime = 60 * 60 * (filter.DepartureTime?.Hour ?? 7) + 60 * (filter.DepartureTime?.Minute ?? 0),
                            Optimize = Optimize.Distance.Description(),
                            DistanceUnit = DistanceUnit.MI.Description(),
                            DeviceType = DeviceType.Web.Description(),
                            SharedPublicly = true,
                            RoundTrip = true

                        };

                        var optimizationParameters = new
                        {
                            addresses = parsedAddresses,
                            parameters = parameters
                        };

                        var createOptimizationRequest = new RestRequest("api.v4/optimization_problem.php", Method.POST);
                        createOptimizationRequest.AddQueryParameter("api_key", c_ApiKey);
                        createOptimizationRequest.AddHeader("Content-Type", "application/json");
                        createOptimizationRequest.AddParameter("application/json", JsonConvert.SerializeObject(optimizationParameters), ParameterType.RequestBody);
                        IRestResponse optimizedResponse = restClient.Execute(createOptimizationRequest);
                        var routeResponse = JsonConvert.DeserializeObject<RouteResponse>(optimizedResponse.Content);
                        foreach (AddressResponse addressResponse in routeResponse.routes.FirstOrDefault()?.addresses)
                        {
                            if(!string.IsNullOrWhiteSpace(addressResponse.Alias))
                            {
                                var soShipmentDbRec = SOShipment.PK.Find(sOShipmentEntry, addressResponse.Alias);
                                if(soShipmentDbRec != null)
                                {
                                    var soShipmentDbRecExt = soShipmentDbRec.GetExtension<SOShipmentExt>();
                                    if(soShipmentDbRecExt!=null)
                                    {
                                        sOShipmentEntry.Document.Current = soShipmentDbRec;
                                        sOShipmentEntry.Document.SetValueExt<SOShipmentExt.usrOptimizationProblemID>(sOShipmentEntry.Document.Current, new Guid(addressResponse.OptimizationProblemID));
                                        sOShipmentEntry.Document.SetValueExt<SOShipmentExt.usrRouteID>(sOShipmentEntry.Document.Current, new Guid(addressResponse.RouteID));
                                        sOShipmentEntry.Document.SetValueExt<SOShipmentExt.usrSequenceNbr>(sOShipmentEntry.Document.Current, Convert.ToInt32(addressResponse.SequenceNumber));
                                        sOShipmentEntry.Document.SetValueExt<SOShipmentExt.usrDistanceToDestination>(sOShipmentEntry.Document.Current, addressResponse.DistanceToNextDestination);
                                        sOShipmentEntry.Document.SetValueExt<SOShipmentExt.usrAbnormalTrafficTimetoDestination>(sOShipmentEntry.Document.Current, (addressResponse.DriveTimeToNextDestination??0)/60);
                                        if(filter.UsrDriver!=null)
                                            sOShipmentEntry.Document.SetValueExt<SOShipmentExt.usrDriver>(sOShipmentEntry.Document.Current, filter.UsrDriver);
                                        sOShipmentEntry.Document.Update(sOShipmentEntry.Document.Current);
                                        sOShipmentEntry.Save.Press();
                                    }
                                }
                            }
                        }

                        string errorString = "";
                        if (string.IsNullOrWhiteSpace(errorString))
                        {
                            PXTrace.WriteError(errorString);
                        }
                    }
                }

            }
            else
            {
                failed = true;
                PXTrace.WriteError("Request Failed");
            }

            if (failed)
            {
                throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
            }
        }

        private static string GetApiKey(SOSetup sOSetup)
        {
            SOSetupExt setupExt = PXCache<SOSetup>.GetExtension<SOSetupExt>(sOSetup);
            if(setupExt != null && setupExt.UsrAPIKey!=null)
            {
                return setupExt.UsrAPIKey;
            }
            return "";
        }
        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (date < dateTime)
            {
                date = new DateTime(1970, 1, 1, date.Hour, date.Minute, date.Second);
            }

            return (long)Math.Floor((date - dateTime).TotalSeconds);
        }
    }
    public class RouteResponse
    {
        public List<MyRoute> routes { get; set; }
    }
    public class MyRoute
    {
        public List<AddressResponse> addresses { get; set; }
    }
    public class AddressResponse
    {
        [JsonProperty("route_destination_id")]
        public int RouteDestinationID { get; set; }
        [JsonProperty("alias")]
        public string Alias { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("is_depot")]
        public bool IsDepot { get; set; }
        [JsonProperty("route_id")]
        public string RouteID { get; set; }
        [JsonProperty("optimization_problem_id")]
        public string OptimizationProblemID { get; set; }
        [JsonProperty("sequence_no")]
        public string SequenceNumber { get; set; }
        [JsonProperty("drive_time_to_next_destination")]
        public int? DriveTimeToNextDestination { get; set; }
        [JsonProperty("abnormal_traffic_time_to_next_destination")]
        public int? AbnormalTrafficTimetoNextDestination { get; set; }
        [JsonProperty("uncongested_time_to_next_destination")]
        public int? UncongestedTimeToNextDestination { get; set; }
        [JsonProperty("traffic_time_to_next_destination")]
        public int? TrafficTimeToNextDestination { get; set; }
        [JsonProperty("distance_to_next_destination")]
        public decimal? DistanceToNextDestination { get; set; }
    }
    public class Route4MeAddress
    {
        public Route4MeAddress()
        {

        }
        [JsonProperty("sequence_no")]
        public int SequenceNo { get; set; }
        [JsonProperty("alias")]
        public string Alias { get; set; }
        [JsonProperty("address")]
        public string AddressString { get; set; }
        [JsonProperty("lat")]
        public string Latitude { get; set; }
        [JsonProperty("lng")]
        public string Longitude { get; set; }
        [JsonProperty("is_depot")]
        public bool IsDepot { get; set; }
        [JsonProperty("lock_last")]
        public bool LockLast { get; set; }
        [JsonProperty("original", NullValueHandling = NullValueHandling.Ignore)]
        public string Original { get; set; }
    }
    public enum Optimize : uint
    {
        [Description("Distance")]
        Distance,
        [Description("Time")]
        Time,
        [Description("timeWithTraffic")]
        TimeWithTraffic
    }
    public enum DeviceType
    {
        [Description("web")]
        Web,
        [Description("iphone")]
        IPhone,
        [Description("ipad")]
        IPad,
        [Description("android_phone")]
        AndroidPhone,
        [Description("android_tablet")]
        AndroidTablet
    }
    public enum DistanceUnit : uint
    {
        [Description("mi")]
        MI,
        [Description("km")]
        KM
    }
    public enum AlgorithmType
    {
        TSP = 1,
        VRP = 2,
        CVRP_TW_SD = 3,
        CVRP_TW_MD = 4,
        TSP_TW = 5,
        TSP_TW_CR = 6,
        ADVANCED_CVRP_TW = 9,
        ALG_NONE = 100,
        ALG_LEGACY_DISTRIBUTED = 101
    }
    public class Route4MeParameters
    {
        [JsonProperty("algorithm_type")]
        public AlgorithmType AlgorithmType { get; set; }
        [JsonProperty("route_name")]
        public string RouteName { get; set; }
        [JsonProperty("route_date")]
        public long RouteDate { get; set; }
        [JsonProperty("route_time")]
        public int RouteTime { get; set; }
        [JsonProperty("optimize")]
        public string Optimize { get; set; }
        [JsonProperty("distance_unit")]
        public string DistanceUnit { get; set; }
        [JsonProperty("device_type")]
        public string DeviceType { get; set; }
        [JsonProperty("shared_publicly")]
        public bool SharedPublicly { get; set; }
        [JsonProperty("rt")]
        public bool RoundTrip { get; set; }
    }
}
