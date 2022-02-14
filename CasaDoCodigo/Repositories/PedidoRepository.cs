using CasaDoCodigo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CasaDoCodigo.Repositories
{
    public interface IPedidoRepository
    {
        Pedido GetPedido();
        void AddItem(string codigo);
    }

    public class PedidoRepository : BaseRepository<Pedido>, IPedidoRepository
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public PedidoRepository(ApplicationContext context, IHttpContextAccessor contextAccessor) : base(context)
        {
            _contextAccessor = contextAccessor;
        }

        public void AddItem(string codigo)
        {
            var produto = _context.Set<Produto>().Where(p => p.Codigo == codigo).SingleOrDefault();
            if(produto == null)
            {
                throw new ArgumentException("Produto Não Encontrado");
            }

            var pedido = GetPedido();
            var itemPedido = _context.Set<ItemPedido>().Where(i => i.Produto.Codigo == codigo && i.Pedido.Id == pedido.Id).SingleOrDefault();

            if (itemPedido == null)
            {
                itemPedido = new ItemPedido(pedido, produto, 1, produto.Preco);
                _context.Set<ItemPedido>().Add(itemPedido);
                _context.SaveChanges();
            }
        }

        public Pedido GetPedido()
        {
            var pedidoId = GetPedidoId();
            var pedido = dbSet
                            .Include(p => p.Itens)
                                .ThenInclude(i => i.Produto)
                            .Where(p => p.Id == pedidoId)
                            .FirstOrDefault();

            if(pedido == null)
            {
                pedido = new Pedido();
                dbSet.Add(pedido);
                _context.SaveChanges();
                SetPedidoId(pedido.Id);
            }

            return pedido;
        }

        private int? GetPedidoId()
        {
            return _contextAccessor.HttpContext.Session.GetInt32("PedidoId");
        }

        private void SetPedidoId(int pedidoId)
        {
            _contextAccessor.HttpContext.Session.SetInt32("PedidoId", pedidoId);
        }
    }
}
