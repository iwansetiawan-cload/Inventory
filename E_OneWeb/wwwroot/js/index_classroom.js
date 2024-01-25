var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/ClassList/GetAll"
        },
        "columns": [
            { "data": "roomname", "autoWidth": true },
            { "data": "locationname", "autoWidth": true },
            { "data": "status", "autoWidth": true },
            { "data": "bookingby", "autoWidth": true },
            { "data": "bookingdate", "autoWidth": true },
            { "data": "bookingenddate", "autoWidth": true },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">                           
                               
                                <a onclick=Delete("/Admin/ClassList/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
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