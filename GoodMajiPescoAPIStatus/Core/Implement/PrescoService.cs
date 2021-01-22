using goodmaji;
using GoodMajiPescoAPIStatus.Core.Logger;
using GoodMajiPescoAPIStatus.Core.Model;
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
   // public string _url = "https://test-cbec.sp88.tw";
   // private readonly string _url = "https://cbec.sp88.tw";

    private readonly APIHelper _apiHelper;
    private readonly DapperHelper _dapperHelper;
    public string _url = System.Configuration.ConfigurationManager.AppSettings["prescourl"];
    public PrescoService()
    {
        //
        // TODO: Add constructor logic here
        //
        _apiHelper = new APIHelper();
        _dapperHelper = new DapperHelper();
    }

    public int UpdateShipmentStatus(List<PrescoShipment> shipStatusRequests)
    {
        var updatelist = GetUpdateShipments(shipStatusRequests);
        var updateCmd = new List<SqlCommand>();

        foreach (var shiphistory in updatelist)
        {
            var shipment = new Shipment {ST12= shiphistory.SSH05, ST02=shiphistory.SSH03 };
            updateCmd.Add(SqlExtension.GetUpdateSqlCmd("Shipment", shipment, new List<string> { "ST02", "ST03","ST13" }, "ST02=@ST02", null));
            updateCmd.Add(InsertShipmentHistory(shiphistory));
        }
        return SqlDbmanager.ExecuteNonQryMutiSqlCmd(updateCmd);
    }

    private SqlCommand InsertShipmentHistory(ShipmentHistory shiphistory)
    {
        var cmd = SqlExtension.GetInsertSqlCmd("ShipmentSTHistory", shiphistory);
        //SSH24.SSH05,SSH03
        cmd.CommandText = @"IF NOT EXISTS(SELECT SSH03 FROM ShipmentSTHistory WHERE SSH24= @SSH24 AND SSH05=@SSH05 AND SSH03=@SSH03) 
                            BEGIN "+ SqlExtension.GetInsertStr("ShipmentSTHistory", shiphistory) + " END ";
        return cmd;
    }
    public List<ShipmentHistory> GetUpdateShipments(List<PrescoShipment> prescoShipments)
    {
        var shipStatusRequests = prescoShipments
                                  .Select(x => new ShipStatusRequest() { ShipNo = x.PrescoShipID })
                                  .ToList();
        var prescoStatus = GetStatusFromPresco(shipStatusRequests);
        var updateList = new List<ShipmentHistory>();
        foreach(var status in prescoStatus.ShipStatuses)
        {
            foreach(var tracking in status.Tracking)
            {
                var goodmajiStatus = MapGoodMajiStatus(tracking.Status);
                if (goodmajiStatus > 0)
                {
                    var prescoshipment = prescoShipments.Where(x => x.PrescoShipID == status.ShipNo).FirstOrDefault();
                    var shipment = new ShipmentHistory { SSH04 = prescoshipment.ParcelNo, SSH05=goodmajiStatus, SSH02 =tracking.Date, SSH03= prescoshipment.GMShipID , SSH25=DateTime.Now, SSH24=tracking.Status.ToString()};
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


    private List<ShipStatus> GetStatus(List<ShipStatusRequest> request)
    {
        var url = _url + "/api/shipment/status";
        var helper = new APIHelper { Url = url, RequestData = JsonConvert.SerializeObject(request), ContentType = "application/json" };
        var rval = new RVal();
        var response = new List<ShipStatus>();

        try
        {
            rval = helper.PostApi();
            AddLog(helper);

            if (rval.RStatus)
                response = JsonConvert.DeserializeObject<List<ShipStatus>>(rval.RMsg);
        }
        catch (Exception ex)
        {
            Logger.AddLog(rval.RMsg, ex.Message);
        }
        return response;
    }

    public List<PrescoShipment>GetShipStatusRequests()
    {
        var sql = "SELECT PrescoOrderLog.PrescoShipID ,PrescoOrderLog.GMShipID, Shipment.ST03 AS ParcelNo FROM Shipment  " +
            "INNER JOIN PrescoOrderLog ON Shipment.ST02 = PrescoOrderLog.GMShipID " +
            "WHERE Shipment.St12 BETWEEN 5 AND 7  AND PrescoOrderLog.Status=1 ";
        return _dapperHelper.Query<PrescoShipment>(sql).ToList();
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


