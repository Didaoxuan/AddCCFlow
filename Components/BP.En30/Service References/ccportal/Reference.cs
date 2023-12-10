﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace BP.En30.ccportal {
    using System.Data;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ccportal.PortalInterfaceSoap")]
    public interface PortalInterfaceSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/CheckUserNoPassWord", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        int CheckUserNoPassWord(string userNo, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetDept", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetDept(string deptNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetDepts", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetDepts();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetDeptsByParentNo", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetDeptsByParentNo(string parentDeptNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetStations", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetStations();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetStation", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetStation(string stationNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetEmps", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetEmps();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetEmpsByDeptNo", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetEmpsByDeptNo(string deptNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetEmp", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetEmp(string no);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetDeptEmp", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetDeptEmp();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetEmpHisDepts", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetEmpHisDepts(string empNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetEmpHisStations", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetEmpHisStations(string empNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetDeptEmpStations", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GetDeptEmpStations();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GenerEmpsByStations", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GenerEmpsByStations(string stationNos);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GenerEmpsByDepts", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GenerEmpsByDepts(string deptNos);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GenerEmpsBySpecDeptAndStats", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        DataTable GenerEmpsBySpecDeptAndStats(string deptNo, string stations);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface PortalInterfaceSoapChannel : BP.En30.ccportal.PortalInterfaceSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PortalInterfaceSoapClient : System.ServiceModel.ClientBase<BP.En30.ccportal.PortalInterfaceSoap>, BP.En30.ccportal.PortalInterfaceSoap {
        
        public PortalInterfaceSoapClient() {
        }
        
        public PortalInterfaceSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public PortalInterfaceSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PortalInterfaceSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PortalInterfaceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public int CheckUserNoPassWord(string userNo, string password) {
            return base.Channel.CheckUserNoPassWord(userNo, password);
        }
        
        public DataTable GetDept(string deptNo) {
            return base.Channel.GetDept(deptNo);
        }
        
        public DataTable GetDepts() {
            return base.Channel.GetDepts();
        }
        
        public DataTable GetDeptsByParentNo(string parentDeptNo) {
            return base.Channel.GetDeptsByParentNo(parentDeptNo);
        }
        
        public DataTable GetStations() {
            return base.Channel.GetStations();
        }
        
        public DataTable GetStation(string stationNo) {
            return base.Channel.GetStation(stationNo);
        }
        
        public DataTable GetEmps() {
            return base.Channel.GetEmps();
        }
        
        public DataTable GetEmpsByDeptNo(string deptNo) {
            return base.Channel.GetEmpsByDeptNo(deptNo);
        }
        
        public DataTable GetEmp(string no) {
            return base.Channel.GetEmp(no);
        }
        
        public DataTable GetDeptEmp() {
            return base.Channel.GetDeptEmp();
        }
        
        public DataTable GetEmpHisDepts(string empNo) {
            return base.Channel.GetEmpHisDepts(empNo);
        }
        
        public DataTable GetEmpHisStations(string empNo) {
            return base.Channel.GetEmpHisStations(empNo);
        }
        
        public DataTable GetDeptEmpStations() {
            return base.Channel.GetDeptEmpStations();
        }
        
        public DataTable GenerEmpsByStations(string stationNos) {
            return base.Channel.GenerEmpsByStations(stationNos);
        }
        
        public DataTable GenerEmpsByDepts(string deptNos) {
            return base.Channel.GenerEmpsByDepts(deptNos);
        }
        
        public DataTable GenerEmpsBySpecDeptAndStats(string deptNo, string stations) {
            return base.Channel.GenerEmpsBySpecDeptAndStats(deptNo, stations);
        }
    }
}
