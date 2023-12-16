var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/RequestItems/GetAllApprove"
        },
        "columns": [
            { "data": "refnumber", "autoWidth": true },
            { "data": "requestdate", "autoWidth": true },
            { "data": "requestby", "autoWidth": true },
            { "data": "totalamount", "autoWidth": true },
            { "data": "status", "autoWidth": true },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                               
                                <a href="/Admin/RequestItems/ViewApprove/${data}" class="btn btn-info" style="cursor:pointer">
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