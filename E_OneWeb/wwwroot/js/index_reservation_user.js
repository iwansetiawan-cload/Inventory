var dataTable;
var dataTable2;

$(document).ready(function () {
    loadDataTable();
    loadDataTable2();
});

function loadDataTable() {
    dataTable = $('#tblDataVehicleReservationUser').DataTable({
        "order": [[8, "desc"]],
        "ajax": {
            "url": "/Users/RoomReservation/GetVehicleReservation"
        },
        "columns": [
            { "data": "name", "autoWidth": true },
            { "data": "bookingdate", "autoWidth": true },
            { "data": "bookingclock", "autoWidth": true },
            { "data": "utility", "autoWidth": true },
            { "data": "destination", "autoWidth": true },
            { "data": "driver", "autoWidth": true },
            { "data": "status", "autoWidth": true },
            { "data": "notes", "autoWidth": true },
            { "data": "id", "autoWidth": true }
        ],
        "createdRow": function (row, data, index) {
            $('td', row).eq(8).attr('style', 'display:none;');
        }
    });
}
function loadDataTable2() {
    dataTable2 = $('#tblDataRoomReservationUser').DataTable({
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
