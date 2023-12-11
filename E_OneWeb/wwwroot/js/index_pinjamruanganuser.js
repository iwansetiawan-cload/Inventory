var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblDataRoomReservationUser').DataTable({
        "ajax": {
            "url": "/Users/RoomReservation/GetAll"
        },
        "columns": [
            { "data": "roomname", "autoWidth": true },
            { "data": "startdate", "autoWidth": true },
            { "data": "enddate", "autoWidth": true },
            { "data": "status", "autoWidth": true }
        ]
    });
}

function Delete(url) {
    swal({
        title: "Are you sure you want to Delete?",
        text: "You will not be able to restore the data!",
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