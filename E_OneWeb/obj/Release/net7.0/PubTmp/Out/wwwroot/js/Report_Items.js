var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "order": [[0, "desc"]],

        "ajax": {
            "url": "/Admin/ReportItems/GetAll"
        },
        "columns": [
            { "data": "code", "autoWidth": true },
            { "data": "name", "autoWidth": true },
            { "data": "description", "autoWidth": true },
            { "data": "category", "autoWidth": true },
            { "data": "room", "autoWidth": true },
            { "data": "price", "autoWidth": true },
            { "data": "qty", "autoWidth": true },
            { "data": "totalamount", "autoWidth": true }
        ],
        "createdRow": function (row, data, index) {
  

        }
    });
}
