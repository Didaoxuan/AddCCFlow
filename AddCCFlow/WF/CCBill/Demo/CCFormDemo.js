//基本操作: 创建一个实体并编辑.
function DemoTest1() {
    var webUser = new WebUser();
    if (webUser.No == null) {
        alert('用户名丢失');
        return;
    }

    //写入一条数据.
    var en = new Entity("Dict_Student");
    en.Name = "张三";
    en.Insert(); //插入到数据库.

    alert('数据已经写入:OID=' + en.OID + '编号:' + en.BillNo);

    //更新操作.
    en.Name = '济南张三';
    en.Update(); //更新到数据.


    //根据主键OID实例化实体. 
    var en = new Entity("Dict_Student", 123);
    alert('学生名称:' + en.Title + " 学生编号:" + en.BillNo);

    //根据学生编号实例化实体. 
    var en = new Entity("Dict_Student", "0001");
    alert('学生名称:' + en.Title + " 学生编号:" + en.BillNo);

    //删除实体.
    var en = new Entity("Dict_Student", "0001");
    var i = en.Delete();
    if (i == 1)
        alert("学生[" + en.Title + "]删除成功");
    else
        alert("学生[" + en.Title + "]删除失败");

    //打开这个实体并编辑方式1.
    var url = basePath + '/WF/Entity/MyDict.htm?FrmID=Student&OID=' + oid;
    window.localhost.href = url;

    //打开这个实体并编辑方式2.
    var url = basePath + '/WF/Entity/MyDict.htm?FrmID=Student&BillNo=0001';
    window.localhost.href = url;
}


function TestFrmAPI() {

    var frmID = "Frm_ChuKuDan";

    //创建一个新纪录. 返回一个oid 类型的数据.
    var oid = CCForm_CreateBlankOID(frmID);


    //打开这个表单
    var url = CCFrom_FrmViewUrl(frmID, oid); //获得url.
    window.open(url); //打开这个url.


    //删除实体记录.
    CCFrom_DeleteFrmEntityByOID(frmID, oid);

}