$(function () {

    $.ajaxSetup({ cache: false });

    $("a[data-modal]").on("click", function (e) {
        // hide dropdown if any
        $(e.target).closest('.btn-group').children('.dropdown-toggle').dropdown('toggle');
        console.log($(e.target).closest('.btn-group').length);
        
        $('#myModalContent').load(this.href, function () {
            if ($('.datepicker').length > 0) {
                $('.datepicker').datepicker({
                    format: "dd/mm/yyyy",
                    autoclose: true
                });
            }

            $('#myModal').modal({
                /*backdrop: 'static',*/
                keyboard: true
            }, 'show');

            bindForm(this);
        });
        
        return false;
    });


});

function show_modal(obj, level) {
    var disabled = $(obj).attr("disabled") == "disabled" ? true : false;

    if (!disabled) {
        var loading = "<div class='modal-body' style='height:250px'><div class='spinner'><div class='bounce1'></div> <div class='bounce2'></div> <div class='bounce3'></div></div></div>";
        if (level == 1) {
            $('#myModalContent').empty();
            $('#myModalContent').append(loading);
            $('#myModal').modal({
                /*backdrop: 'static',*/
                keyboard: true
            }, 'show');

            $('#myModalContent').load(obj.href, function () {
                if ($('.datepicker').length > 0) {
                    $('.datepicker').datepicker({
                        format: "dd/mm/yyyy",
                        autoclose: true
                    });
                }

                //$('#myModal').modal({
                //    /*backdrop: 'static',*/
                //    keyboard: true
                //}, 'show');

                bindForm(this);
            });
        } else {
            $('#myModalContent' + level).empty();
            $('#myModalContent' + level).append(loading);
            $('#myModal' + level).modal({
                /*backdrop: 'static',*/
                keyboard: true
            }, 'show');

            $('#myModalContent' + level).load(obj.href, function () {
                if ($('.datepicker').length > 0) {
                    $('.datepicker').datepicker({
                        format: "dd/mm/yyyy",
                        autoclose: true
                    });
                }

                //$('#myModal' + level).modal({
                //    /*backdrop: 'static',*/
                //    keyboard: true
                //}, 'show');

                bindForm(this);
            });
        }
    }

    return false;
}

$('#myModal').on('shown.bs.modal', function () {
    /*
    var dv = $(".divScrollCheckBox");
    var label = $(dv).find('label');
    
    $.each(label, function () {
        $(this).after("<br>");
    });
    */
})

function bindForm(dialog) {
    
    $('form', dialog).submit(function () {
        var isModal = $(this).parents("div[class^=modal]");
        var async = $(this).attr("is-async") == "false" ? false : true;

        if ($(isModal).length && async) {
            var modal = $(isModal).attr("id");
            var modal_content = $(isModal).find("div[id^=myModalContent]").attr("id");

            var url = $(this).attr('action');
            var method = this.method;
            var data = $(this).serialize();

            var msg = "url=" + url + "&type=" + method + "&async=" + async;
            console.log(msg);
            //console.log(data);

            $.ajax({
                url: url,
                type: method,
                data: data,
                success: function (result) {
                    if (result.success) {
                        $('#' + modal).modal('hide');
                        //Refresh
                        location.reload();
                    } else {
                        $('#' + modal_content).html(result);
                        bindForm();
                    }
                }
            });
            return false;
        }
    });
}

function submitModal(obj) {
    try {
        //alert(this.herf);
        //$.ajaxSetup({ cache: false });

        //// hide dropdown if any
        ////$(obj.target).closest('.btn-group').children('.dropdown-toggle').dropdown('toggle');
        //$(obj.target).closest('.btn-group').children('.dropdown-toggle').dropdown('toggle');
        //alert(this.href);
        $('#myModalContent').load("http://localhost:61430/SysUser/MultipleEdit", function () {


            $('#myModal').modal({
                /*backdrop: 'static',*/
                keyboard: true
            }, 'show');

            bindForm2(this);
        });

        return false;

    } catch (e) {
        alert(e);
    }
};

function showModal(obj) {
    try {

        $.ajaxSetup({ cache: false });

        // hide dropdown if any
        //$(obj.target).closest('.btn-group').children('.dropdown-toggle').dropdown('toggle');
       
        $('#myModalContent2').load(obj.href, function () {


            $('#myModal2').modal({
                /*backdrop: 'static',*/
                keyboard: true
            }, 'show');

            bindForm2(this);
        });

        return false;

    } catch (e) {
        alert(e);
    }
};

function bindForm2(dialog) {

    $('form', dialog).submit(function () {
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (result) {
                if (result.success) {
                    $('#myModal2').modal('hide');
                    //Refresh
                    location.reload();
                } else {
                    $('#myModalContent2').html(result);
                    bindForm();
                }
            }
        });
        return false;
    });
}
