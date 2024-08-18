var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "order": [[0, "desc"]],

        "ajax": {
            "url": "/Admin/Items/GetAll"
        },
        "columns": [
            { "data": "id", "autoWidth": true },
            { "data": "code", "autoWidth": true },
            { "data": "name", "autoWidth": true },       
            { "data": "startdate", "autoWidth": true }, 
            { "data": "room", "autoWidth": true },
            { "data": "category", "autoWidth": true },
            { "data": "ownership", "autoWidth": true },
            { "data": "price", "autoWidth": true },
            { "data": "qty", "autoWidth": true },
            
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Items/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>    
                                <a onclick=Delete("/Admin/Items/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i> 
                                </a>
                            </div>
                           `;
                }, "autoWidth": true
            }
        ],
        "createdRow": function (row, data, index) {
            $('td', row).eq(0).attr('style', 'display:none;');
       
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