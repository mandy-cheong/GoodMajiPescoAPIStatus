using goodmaji;
using Newtonsoft.Json;
using SqlLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for PrescoService
/// </summary>
public class PrescoService
{
    //private readonly string _url = "https://cbec-test.sp88.tw";
    private readonly string _url = "https://cbec.sp88.tw";
    private readonly APIHelper _apiHelper;
    private readonly DapperHelper _dapperHelper;
    
    public PrescoService()
    {
        //
        // TODO: Add constructor logic here
        //
        _apiHelper = new APIHelper();
        _dapperHelper = new DapperHelper();
    }

    public int UpdateShipmentStatus(List<ShipStatusRequest> shipStatusRequests)
    {
        var updatelist = GetUpdateShipments(shipStatusRequests);
        var updateCmd = new List<SqlCommand>();

        foreach (var shipment in updatelist)
        {
            updateCmd.Add(SqlExtension.GetUpdateSqlCmd("Shipment", shipment, new List<string> { "ST69" }, "ST69=@ST69", null));
        }
        return SqlDbmanager.ExecuteNonQryMutiSqlCmd(updateCmd);
    }
    public List<Shipment> GetUpdateShipments(List<ShipStatusRequest> shipStatusRequests)
    {
        var prescoStatus = GetStatusFromPresco(shipStatusRequests);
        var updateList = new List<Shipment>();
        foreach(var status in prescoStatus.ShipStatuses)
        {
            foreach(var tracking in status.Tracking)
            {
                var goodmajiStatus = MapGoodMajiStatus(tracking.Status);
                if (goodmajiStatus > 0)
                {
                    var shipment = new Shipment { ST12 = goodmajiStatus, ST69 = status.ShipNo };
                    updateList.Add(shipment);
                }
            }
        }
        return updateList;
    }
    public ShipStatusResponse GetStatusFromPresco(List<ShipStatusRequest> shipStatusRequests)
    {
        int maxRequest = 100;
        int counter = (shipStatusRequests.Count + maxRequest - 1) / maxRequest;
        var result = new ShipStatusResponse();

        for (int i = 1; i <= counter; i++)
        {
            var skiprecord = 100 * (i - 1);
            var request = shipStatusRequests.Take(maxRequest).Skip(skiprecord).ToList();
            result.ShipStatuses.AddRange(GetStatus(request));
        }

        return result;
    }

    private int MapGoodMajiStatus(string status)
    {
        switch (status)
        {
            case PrescoShipStatus.DeliveryCompleted:
                return (int)GoodMajiStatus.DeliveryCompleted;
            case PrescoShipStatus.ArrivedAtDistributionPoint:
                return (int)GoodMajiStatus.ArrivedAtDistributionPoint;
            case PrescoShipStatus.RTS:
                return (int)GoodMajiStatus.ArrivedAtDistributionPoint;
            default:
                return 0;
        }
    }
    

    private List<ShipStatus> GetStatus( List<ShipStatusRequest> request)
    {
        var url = _url + "/api/shipment/status";
        var helper = new APIHelper { Url = url, RequestData = JsonConvert.SerializeObject(request), ContentType= "application/json" };
        var rval = helper.PostApi();
        AddLog(helper);

        if (rval.RStatus)
        {
            var response = JsonConvert.DeserializeObject<List<ShipStatus>>(rval.RMsg);
            return response;
        }
        else
        {
            return new List<ShipStatus>();
        }
    }

    public List<ShipStatusRequest>GetShipStatusRequests()
    {
        var sql = "SELECT ST69 As ShipNo FROM Shipment  INNER JOIN PrescoOrderLog ON Shipment.ST69 = PrescoOrderLog.GMShipID WHERE Shipment.St12>5";
        return _dapperHelper.Query<ShipStatusRequest>(sql).ToList();
    }
    private bool AddLog(APIHelper helper)
    {
        PrescoAPILog prescoAPILog = MapAPILog(helper);
        var cmd = SqlExtension.GetInsertSqlCmd("PrescoAPILog", prescoAPILog);
        return SqlHelper.executeNonQry(cmd);
    }
    private PrescoAPILog MapAPILog(APIHelper helper)
    {
        return new PrescoAPILog
        {
            SysId = Guid.NewGuid(),
            CDate = DateTime.Now,
            URL = helper.Url,
            RequestData = helper.RequestData,
            ResponseData = helper.ResponseData
        };
    }
}


