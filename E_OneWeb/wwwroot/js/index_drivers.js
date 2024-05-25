var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "order": [[0, "asc"]],
        "ajax": {
            "url": "/Admin/Drivers/GetAll"
        },
        "columns": [
            { "data": "name", "autoWidth": true },
            { "data": "address", "autoWidth": true },
            { "data": "phone", "autoWidth": true },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Drivers/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>    
                                <a onclick=Delete("/Admin/Drivers/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i> 
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