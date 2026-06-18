using Microsoft.EntityFrameworkCore;
using gschatbot.api.Models;

namespace gschatbot.api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Especialidade> Especialidades { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }
    public DbSet<Especialista> Especialistas { get; set; }
    public DbSet<EspecialistaEndereco> EspecialistasEnderecos { get; set; }
public DbSet<HorarioConsulta> HorariosConsulta { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Agendamento> Agendamentos { get; set; }
    public DbSet<HistoricoMensagem> HistoricoMensagens { get; set; }
    public DbSet<SessaoConversa> SessoesConversa { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Especialidade -> Especialista (1:N)
        modelBuilder.Entity<Especialista>()
            .HasOne(e => e.Especialidade)
            .WithMany(esp => esp.Especialistas)
            .HasForeignKey(e => e.EspecialidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        // EspecialistaEndereco (N:N entre Especialista e Endereco)
        modelBuilder.Entity<EspecialistaEndereco>()
            .HasOne(ee => ee.Especialista)
            .WithMany(e => e.EspecialistasEnderecos)
            .HasForeignKey(ee => ee.EspecialistaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EspecialistaEndereco>()
            .HasOne(ee => ee.Endereco)
            .WithMany(end => end.EspecialistasEnderecos)
            .HasForeignKey(ee => ee.EnderecoId)
            .OnDelete(DeleteBehavior.Cascade);

// HorarioConsulta -> Especialista (1:N)
        modelBuilder.Entity<HorarioConsulta>()
            .HasOne(hc => hc.Especialista)
            .WithMany(e => e.HorariosConsulta)
            .HasForeignKey(hc => hc.EspecialistaId)
            .OnDelete(DeleteBehavior.Cascade);

        // HorarioConsulta -> Endereco (1:N)
        modelBuilder.Entity<HorarioConsulta>()
            .HasOne(hc => hc.Endereco)
            .WithMany(end => end.HorariosConsulta)
            .HasForeignKey(hc => hc.EnderecoId)
            .OnDelete(DeleteBehavior.Restrict);

        // HorarioConsulta <-> Agendamento (1:1)
        modelBuilder.Entity<Agendamento>()
            .HasOne(a => a.HorarioConsulta)
            .WithOne(hc => hc.Agendamento)
            .HasForeignKey<Agendamento>(a => a.HorarioConsultaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Agendamento -> Cliente (N:1)
        modelBuilder.Entity<Agendamento>()
            .HasOne(a => a.Cliente)
            .WithMany(c => c.Agendamentos)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Agendamento -> Especialista (N:1)
        modelBuilder.Entity<Agendamento>()
            .HasOne(a => a.Especialista)
            .WithMany(e => e.Agendamentos)
            .HasForeignKey(a => a.EspecialistaId)
            .OnDelete(DeleteBehavior.Restrict);

        // HistoricoMensagem -> Cliente (N:1)
        modelBuilder.Entity<HistoricoMensagem>()
            .HasOne(hm => hm.Cliente)
            .WithMany(c => c.HistoricoMensagens)
            .HasForeignKey(hm => hm.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // HistoricoMensagem -> Agendamento (N:1, opcional)
        modelBuilder.Entity<HistoricoMensagem>()
            .HasOne(hm => hm.Agendamento)
            .WithMany(a => a.HistoricoMensagens)
            .HasForeignKey(hm => hm.AgendamentoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // SessaoConversa <-> Cliente (1:1)
        modelBuilder.Entity<SessaoConversa>()
            .HasOne(sc => sc.Cliente)
            .WithOne(c => c.Sessao)
            .HasForeignKey<SessaoConversa>(sc => sc.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices para performance
        modelBuilder.Entity<HorarioConsulta>()
            .HasIndex(hc => new { hc.EspecialistaId, hc.DataConsulta });

        modelBuilder.Entity<Agendamento>()
            .HasIndex(a => a.ClienteId);

        modelBuilder.Entity<Agendamento>()
            .HasIndex(a => a.Status);

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.NumeroWhatsApp)
            .IsUnique();
    }
}