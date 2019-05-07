using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mohome_api.ViewModels
{
    public class ErrorDetails
    {
        public int errorId { get; set; }
        public string errorMessage { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(new { error = this });
        }
    }
}
