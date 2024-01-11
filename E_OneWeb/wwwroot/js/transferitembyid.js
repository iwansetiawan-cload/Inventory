var dataTable;

$(document).ready(function () {
    var ItemID = $('#itemID').val();
    loadDataTable(ItemID);
});


function loadDataTable(ItemID) {
   // var ItemID = $('#itemID').val();
    dataTable = $('#tblDataTransfer').DataTable({
        "ajax": {
            "url": "/Admin/Items/GetTransferItem",
            "data": {
                "id": ItemID
            }
        },
        "columns": [
            { "data": "stranferdate", "width": "20%" },
          
            { "data": "previouslocation", "width": "20%" },
            { "data": "currentlocation", "width": "20%" },
            { "data": "desc", "width": "30%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">                                
                                <a onclick=Delete("/Admin/ItemTransfer/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i> 
                                </a>
                            </div>
                           `;
                }, "width": "40%"
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