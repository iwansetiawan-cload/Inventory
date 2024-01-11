var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/RequestItems/GetAll"
        },
        "columns": [
            { "data": "refnumber", "autoWidth": true },
            { "data": "requestdate", "autoWidth": true },
            { "data": "requestby", "autoWidth": true },
           /* { "data": "desc", "width": "20%" },*/
            { "data": "totalamount", "autoWidth": true },
            { "data": "status", "autoWidth": true },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                               
                                <a href="/Admin/RequestItems/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>   
                               
                                <a onclick=Delete("/Admin/RequestItems/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i> 
                                </a>
                            </div>
                           `;
                }, "autoWidth": true
            }
        ]
    });
}

 //<a href="/Admin/RequestItems/downloadFile/${data}" class="btn btn-primary text-white" style="cursor:pointer">
                
                                //    <i class="fas fa-file-export"></i>
                                //</a>  

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