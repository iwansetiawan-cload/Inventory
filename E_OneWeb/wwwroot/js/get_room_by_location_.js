﻿var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblDataRoom').DataTable({
        "ajax": {
            "url": "/Admin/Room/GetAllRoom"
        },
        "columns": [
            { "data": "name", "autoWidth": true },
            { "data": "description", "autoWidth": true },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Room/UpsertRoom/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>  
                                 <a onclick=Delete("/Admin/Room/DeleteRoom/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i> 
                                </a>
                                 <a href="/Admin/Room/ViewItemList/${data}" class="btn btn-info" style="cursor:pointer">
                                    <i class="fas fa-eye"></i>
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