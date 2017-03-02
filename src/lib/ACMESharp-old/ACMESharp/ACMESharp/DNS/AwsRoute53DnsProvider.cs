﻿using System.Collections.Generic;

namespace ACMESharp.DNS {
 public class AwsRoute53DnsProvider : IXXXDnsProvider {
  public System.String HostedZoneId { get; set; }

  public System.String AccessKeyId { get; set; }

  public System.String SecretAccessKey { get; set; }

  public System.String Region {
   get => RegionEndpoint?.SystemName; set => RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName ( value );
  }

  public Amazon.RegionEndpoint RegionEndpoint { get; set; } = Amazon.RegionEndpoint.USEast1;

  public void EditTxtRecord ( System.String dnsName, IEnumerable<System.String> dnsValues ) {
   var dnsValuesJoined = System.String.Join ( "\" \"", dnsValues );
   var rrset = new Amazon.Route53.Model.ResourceRecordSet {
    TTL = 30,
    Name = dnsName,
    Type = Amazon.Route53.RRType.TXT,
    ResourceRecords = new List<Amazon.Route53.Model.ResourceRecord>
       {
                    new Amazon.Route53.Model.ResourceRecord(
                            $"\"{dnsValuesJoined}\"")
                }
   };

   EditR53Record ( rrset );
  }

  public void EditARecord ( System.String dnsName, System.String dnsValue ) {
   var rrset = new Amazon.Route53.Model.ResourceRecordSet {
    TTL = 30,
    Name = dnsName,
    Type = Amazon.Route53.RRType.A,
    ResourceRecords = new List<Amazon.Route53.Model.ResourceRecord>
       {
                    new Amazon.Route53.Model.ResourceRecord(dnsValue)
                }
   };

   EditR53Record ( rrset );
  }

  public void EditCnameRecord ( System.String dnsName, System.String dnsValue ) {
   var rrset = new Amazon.Route53.Model.ResourceRecordSet {
    TTL = 30,
    Name = dnsName,
    Type = Amazon.Route53.RRType.CNAME,
    ResourceRecords = new List<Amazon.Route53.Model.ResourceRecord>
       {
                    new Amazon.Route53.Model.ResourceRecord(dnsValue)
                }
   };

   EditR53Record ( rrset );
  }

  private void EditR53Record ( Amazon.Route53.Model.ResourceRecordSet rrset ) {
   var r53 = new Amazon.Route53.AmazonRoute53Client (
           AccessKeyId, SecretAccessKey, RegionEndpoint );

   var rrRequ = new Amazon.Route53.Model.ChangeResourceRecordSetsRequest {
    HostedZoneId = HostedZoneId,
    ChangeBatch = new Amazon.Route53.Model.ChangeBatch {
     Changes = new List<Amazon.Route53.Model.Change>
           {
                        new Amazon.Route53.Model.Change
                        {
                            Action = Amazon.Route53.ChangeAction.UPSERT,
                            ResourceRecordSet = rrset
                        }
                    }
    }
   };
   var rrResp = r53.ChangeResourceRecordSets ( rrRequ );
  }
 }
}