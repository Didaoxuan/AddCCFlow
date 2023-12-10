
function NewFlow(flowNo) {
    var url = "../MyFlow.htm?FK_Flow=" + flowNo;
    window.open(url);
    return;
}

function DealFlowEmps(str) {
    return str;
}



function NewFlowTemplate() {
    var url = "/WF/Comm/Search.htm?EnsName=BP.Cloud.Template.FlowExts";
    var url = "/App/FlowDesigner/NewFlow.htm?EnsName=BP.Cloud.Template.FlowExts";

    SetHref(url);
}

function ImpFlowTemplate() {

    //alert();
    //var url = "/App/FlowDesigner/NewFlow/Default.htm";
    var url = "/WF/Comm/Search.htm?EnsName=BP.Cloud.Template.FlowExts";
    var url = "/App/FlowDesigner/Template.htm?EnsName=BP.Cloud.Template.FlowExts";
    SetHref(url);
}
function DelTodoEmps(str) {
    return str;
}
function DealFlowEmps(str) {
    var result = str.split("@");
    var reg = /(([a-zA-Z\,]+)([0-9]+))|([0-9]+)|([a-zA-Z\,]+)|([a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(.[a-zA-Z0-9_-]+)+$)/;
    var idx = 0;
    while (result = str.match(reg)) {
        if (idx == 0)
            str = str.replace(result[0], '').replace("@", ";");
        else
            str = str.replace(result[0], '').replace("@", ";");
        idx++;
    }
    return str;
}
