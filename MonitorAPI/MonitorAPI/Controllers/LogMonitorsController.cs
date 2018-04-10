using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MonitorAPI.Models;

namespace MonitorAPI.Controllers
{
    public class LogMonitorsController : ApiController
    {
        private MonitorAPIContext db = new MonitorAPIContext();

        // GET: api/LogMonitors
        public IQueryable<LogMonitor> GetLogMonitors()
        {
            return db.LogMonitors;
        }

        // GET: api/LogMonitors/5
        [ResponseType(typeof(LogMonitor))]
        public async Task<IHttpActionResult> GetLogMonitor(int id)
        {
            LogMonitor logMonitor = await db.LogMonitors.FindAsync(id);
            if (logMonitor == null)
            {
                return NotFound();
            }

            return Ok(logMonitor);
        }

      

        // POST: api/LogMonitors
        [ResponseType(typeof(LogMonitor))]
        public async Task<IHttpActionResult> PostLogMonitor(LogMonitor logMonitor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.LogMonitors.Add(logMonitor);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = logMonitor.Id }, logMonitor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LogMonitorExists(int id)
        {
            return db.LogMonitors.Count(e => e.Id == id) > 0;
        }
    }
}