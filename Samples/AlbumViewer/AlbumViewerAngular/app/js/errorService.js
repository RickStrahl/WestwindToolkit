(function () {
    'use strict';

    angular
        .module('app')
        .service('errorService', errorService);

    errorService.$inject = [];

    function errorService() {
        var vm = {
            message: null,
            icon: "warning",
            reset: function() {
                vm.message = null;
                vm.icon = "warning";
            },
            error: function(message, icon) {
                vm.error.reset();
                vm.error.message = message;
                if (!icon)
                    icon = "error";

                vm.icon = icon;
            },
            info: function(message, icon) {
                vm.reset();
                vm.message = message;
                if (!icon)
                    icon = "info";
                vm.icon = icon;
            },            
            parseHttpError: function() {                
                var args = arguments;

                // error/message object passed rather than parm object
                if (args.hasOwnProperty("message")) {
                    vm.message = args.message;
                    return;
                }
                if (args.hasOwnProperty("Message")) {
                    vm.message = args.Message;
                    return;
                }

                var data = args[0]; // http content
                var status = args[1];
                var msg = args[2];
                if(typeof msg != "String")
                   msg = null;

                if (data) {                    
                    if (data.hasOwnProperty("message")) {
                        vm.message = data.message;
                        return;
                    }
                    if (data.hasOwnProperty("Message")) {
                        vm.message = data.Message;
                        return;
                    }

                    // assume JSON   
                    try {
                        var msg = JSON.parse(data);
                        if (msg && msg.hasOwnProperty("message"))
                            vm.message = msg.message;
                        else if (msg.hasOwnProperty("Message"))
                            vm.message = msg.Message;

                        if(vm.message)
                            return;
                    } catch (exception) {}
                }
                if (!msg) {
                    if (status === 404)
                        msg = "URL not found.";
                    else if (status === 401)
                        msg = "Not authorized.";
                    else
                        msg = "Unknown error. Status: " + status;
                }
                vm.message = msg;
            }
        };

        this.error = vm;
    }
})();