
    
$(document).ready(
    function (){

        //----------Show tables--------------------
        var api_id = 'f0ce8vnzeh';
        var region = 'us-east-2'
      $.ajax({
            type: 'GET',
            url: 'https://'+api_id+'.execute-api.'+region+'.amazonaws.com/prod/',
            success: function(data){   
                $.each(data, function(index, item){
                    $('#tblInfoTable tbody:last').append('<tr>'+
                                                    '<th scope="row">'+(index+1)+'</th>'+
                                                    '<td>'+item.tableName+'</td>'+
                                                    '<td>'+item.hashKey+'</td>'+
                                                    '<td>'+item.rangeKey+'</td>'+
                                                    '<td><button id="a'+index+'"class="btn btn-primary">Delete</button></td>'+
                                                        '</tr>');
                });
            },
            error: function(){
                alert("Sorry! Error loading tables...");
            }
        });
    
    
        
    
        //---------Create new table-------------------------------------
        
        $('#btnAddTable').on('click', function(){

            var table = {
                tableName: $('#inputTableName').val(),
                partitionKey: $('#inputHashKey').val(),
                partitionKeyType: $('#inputHKType').val(),
                sortKey: ($('#inputRangeKey').val() == "") ? null : $('#inputRangeKey').val(),
                sortKeyType: $('#inputRKType').val(),
                readCapacityUnits: 5,
                writeCapacityUnits: 5
            }
    
            $.ajax({
    
                type: 'POST',
                url: 'https://'+api_id+'.execute-api.'+region+'.amazonaws.com/prod/',
                data : JSON.stringify(table),
                async: true,
                success: function(){
                    //Bug: success event not fired
                },
                error: function(xhr){
                    if(xhr.status == 200)
                    {
                        var idx = parseInt($('#tblInfoTable tbody:last tr:last th').text()) + 1
                        $('#tblInfoTable tbody:last').append('<tr>'+
                                    '<th scope="row">'+idx+'</th>'+
                                    '<td>'+table.tableName+'</td>'+
                                    '<td>'+table.partitionKey+'</td>'+
                                    '<td>'+table.sortKey+'</td>'+
                                    '<td><button class="btn btn-primary">Delete</button></td>'+
                            '</tr>');
                         $('#frmContainer').hide();
                    }else{
                        alert('Error creating table. Try again!')
                    }
                }
            });
        });
    
    
    
    
        //---------Delete table-----------------------------------------
         
        $('#tblInfoTable').on('click','button',function(){
            var pos = $(this).closest('tr');
            var tbname = pos.children('td:first').text();

            $.ajax({
                type: 'DELETE',
                url: 'https://'+api_id+'.execute-api.'+region+'.amazonaws.com/prod/?tableName='+tbname,
                success:function(){
                     //Bug: success event not fired
                },
                error: function(xhr){
                    if(xhr.status == 200){
                        pos.remove();
          
                        $.each($('#tblInfoTable tbody:last th'), function(idx, itm){
                            $(this).text(idx+1)
                        });
                       
                        alert('Removed table');
                    }
                    else{
                        alert('Error removing table...');
                    }
                }
            })
          
        }); 
        
        

        //--------------Hide forms--------------------------------------

        $('#frmContainer').hide();
        $('#frmTableDetail').hide();



        //-----------open form create table------------------------------------

        $('#btnCreateTable').on('click', function(){
            $('#frmContainer').show();
        });



        //-----------------close form create table------------------------------

        $('#btnCloseFrm').on('click', function(){
            $('#frmContainer').hide();
        });




        //---------------------close form table detail-----------------------------------

        $('#btnCloseTableDetail').on('click', function(){
            $('#frmTableDetail').hide();
        });



        //-----------------open form table detail-------------------------------------------
        
        $('#tblInfoTable tbody:last').on('click','tr td:not(:last-child)',function(){
            var tr = $(this).closest('tr');
            var tableName = $(tr).children('td:first').text()
            var hashKey = $(tr).children('td:eq(1)').text()
            var rangeKey = $(tr).children('td:eq(2)').text()

            $('#tbName').text(tableName)
            $('#hKey').text(hashKey)
            $('#rKey').text(rangeKey)
            
            $.ajax({
                type: 'GET',
                url: 'https://'+api_id+'.execute-api.'+region+'.amazonaws.com/prod/getattr?tableName='+tableName,
                success: function(data){
                    $('#tblAttr tbody').text('')
                    var attrName = Object.keys(data['attr'])
                    $.each(attrName, function(index, item){
                        var type = data['attr'][item]['type']['Value']
                        $('#tblAttr tbody').append(
                            '<tr><th scope="row">'+(index+1)+'</th>'+
                            '<td>'+item+'</td>'+
                            '<td>'+type+'</td></tr>'
                        );
                    });
                    $('#frmTableDetail').show();
                },
                error: function(){
                    alert('Error loading attributes of table '+tableName+'.Try again!');
                }
            })
        });



        //-------Copy link ----------------------------------------------
        
        $('#frmTableDetail button').on('click', function(){
            copyToClipboard($(this).siblings('p'));
        });

        //////
        function copyToClipboard(element) {
            var $temp = $("<input>");
            $("body").append($temp);
            $temp.val($(element).text()).select();
            document.execCommand("copy");
            $temp.remove();
        }


        //--------Show catalog-------------------------------------

        $('#frmTableDetail button.ctl').on('click', function(){
            $(this).closest('div.mb-3').children('div.img').toggleClass('display--none')
        });
    }
);
        
       
            
          /*  */
