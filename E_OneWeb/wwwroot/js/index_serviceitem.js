var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/ServiceItems/GetAll"
        },
        "columns": [
            { "data": "itemname", "autoWidth": true },
            { "data": "location", "autoWidth": true },
            { "data": "servicedate", "autoWidth": true },
            { "data": "serviceenddate", "autoWidth": true },
            { "data": "requestby", "autoWidth": true },
            { "data": "costofrepair", "autoWidth": true },
            { "data": "status", "autoWidth": true },
            { "data": "notes", "autoWidth": true },
            { "data": "approveby", "autoWidth": true },
            { "data": "approvedate", "autoWidth": true },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/ServiceItems/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>    
                                <a onclick=Delete("/Admin/ServiceItems/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i> 
                                </a>
                            </div>
                           `;
                }, "autoWidth": true
            }
        ],
        "createdRow": function (row, data, index) {
            if (data["status"] == 'Approved' || data["status"] == 'Rejected') {
                $('td', row).eq(10).html('');
            }
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