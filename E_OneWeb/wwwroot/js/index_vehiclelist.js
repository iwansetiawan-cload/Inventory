var dataTable;

$(document).ready(function () {
   
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblDataVehicleList').DataTable({
       /* "order": [[5, "asc"]],*/
        "ajax": {
            "url": "/Admin/VehicleList/GetAll"
        },
        "columns": [
            { "data": "code", "autoWidth": true },
            { "data": "name", "autoWidth": true },
            { "data": "room", "autoWidth": true },
            { "data": "location", "autoWidth": true },
            {
                "data": {
                    id: "id", idreservation: "idreservation"
                },
                "render": function (data) {
                    //var today = new Date().getTime();
                    var idresev = data.idreservation;
                    if (idresev > 0) {
                        //user is currently locked
                        return `
                            <div class="text-center">
                                <a onclick=Unlock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                                    <i class="fas fa-lock-open"></i>  Unlock
                                </a>
                            </div>
                           `;
                    }
                    else {
                        return `
                            <div class="text-center">
                                <a onclick=Lock('${data.id}') class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                                    <i class="fas fa-lock"></i>  Lock
                                </a>
                            </div>
                           `;
                    }
                }, "autoWidth": true
            }
        ]
    });
}

function Lock(id) {

    $.ajax({
        type: "POST",
        url: '/Admin/VehicleList/Lock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    });

}

function Unlock(id) {

    $.ajax({
        type: "POST",
        url: '/Admin/VehicleList/Unlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    });

}