var dataTable;

//$(document).ready(function () {
   
//    loadDataTable();
//});
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("avalable")) {
        loadDataTable("GetOrderList?status=avalable");
    }
    else {
        if (url.includes("waiting_approval")) {
            loadDataTable("GetOrderList?status=waiting_approval");
        }
        else {
            if (url.includes("room_in_use")) {
                loadDataTable("GetOrderList?status=room_in_use");
            }
            else {             
                loadDataTable("GetOrderList?status=all");
            }
        }
    }
});


function loadDataTable(url) {
    dataTable = $('#tblDataRoomReservationAdmin').DataTable({
        "ajax": {
           /* "url": "/Admin/RoomReservationAdmin/GetAll"*/
            "url": "/Admin/RoomReservationAdmin/" + url
        },
        "columns": [
            { "data": "roomname", "autoWidth": true },
            { "data": "locationname", "autoWidth": true },
            { "data": "status", "autoWidth": true },
            { "data": "bookingby", "autoWidth": true },
            { "data": "bookingdate", "autoWidth": true },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/RoomReservationAdmin/Delete/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    Approve
                                </a>    
                                
                            </div>
                           `;
                   
                }, "autoWidth": true
            }
        ]
    });
}

function Delete(url) {
    swal({
        title: "Apakah Anda yakin ingin Menghapus?",
        text: "Anda tidak akan dapat memulihkan data!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
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
    });
}