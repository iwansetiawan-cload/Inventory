var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblDataRoomReservationUser').DataTable({
        "order": [[5, "desc"]],
        "ajax": {
            "url": "/Users/RoomReservation/GetAll"
        },
        "columns": [
            { "data": "roomname", "autoWidth": true },
            { "data": "bookingdate", "autoWidth": true },
            { "data": "bookingclock", "autoWidth": true },
            { "data": "status", "autoWidth": true },
            { "data": "notes", "autoWidth": true },
            { "data": "id", "autoWidth": true }
        ],
        "createdRow": function (row, data, index) {
            $('td', row).eq(5).attr('style', 'display:none;');
}
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