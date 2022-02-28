using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using CartAPI.Models;
//using RabbitMQ.Client;

namespace CartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartssController : ControllerBase
    {
        private readonly CartDBContext _context;
        private readonly IConfiguration env;

        public CartssController(CartDBContext context, IConfiguration env)
        {

            _context = context;
            this.env = env;
        }

        // GET: api/Carts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
        {
            return await _context.Carts.ToListAsync();
        }

        // GET: api/Carts/5
        [HttpGet("{Cart_ID}")]
        public async Task<ActionResult<Cart>> GetCart(int Cart_ID)
        {
            var cart = await _context.Carts.FindAsync(Cart_ID);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{Cart_ID}")]
        public async Task<IActionResult> PutCart(int Cart_ID, Cart cart)
        {
            if (Cart_ID != cart.Cart_ID)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(Cart_ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Carts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cart>> PostCart(Cart cart)
        {
            cart.Order_Status = "INITIATED";
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            string message = JsonConvert.SerializeObject(cart);

            SPT.RabbitMQProducer.SendToRabbitMQ("orders", message, env);

            return CreatedAtAction("GetCart", new { Cart_ID = cart.Cart_ID }, cart);
        }

        // DELETE: api/Carts/5
        [HttpDelete("{Cart_ID}")]
        public async Task<IActionResult> DeleteCart(int Cart_ID)
        {
            var cart = await _context.Carts.FindAsync(Cart_ID);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(int Cart_ID)
        {
            return _context.Carts.Any(e => e.Cart_ID == Cart_ID);
        }
    }
}
