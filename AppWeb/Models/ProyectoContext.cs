using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Models;

public partial class ProyectoContext : DbContext
{
    public ProyectoContext()
    {
    }

    public ProyectoContext(DbContextOptions<ProyectoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Imagen> Imagens { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.EstadoId).HasName("PK_estado_FEF86B006A3B2AA3");

            entity.ToTable("estado");

            entity.Property(e => e.NombreEstado)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("nombre_estado");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK_Events_7944C810862C412F");

            entity.Property(e => e.End).HasColumnType("datetime");
            entity.Property(e => e.Start).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Estado).WithMany(p => p.Events)
                .HasForeignKey(d => d.EstadoId)
                .HasConstraintName("FK_Estado");

            entity.HasOne(d => d.Pago).WithMany(p => p.Events)
                .HasForeignKey(d => d.PagoId)
                .HasConstraintName("FK_Pago");

            entity.HasOne(d => d.User).WithMany(p => p.Events)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Usuario");

            entity.HasOne(d => d.Vehiculo).WithMany(p => p.Events)
                .HasForeignKey(d => d.VehiculoId)
                .HasConstraintName("FK_Vehiculo");
        });

        modelBuilder.Entity<Imagen>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK_imagen_B42D8F2AD4E5C72B");

            entity.ToTable("imagen");

            entity.Property(e => e.ImagenMimeType).HasMaxLength(255);
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.PagoId).HasName("PK_Pago_F00B61381932F248");

            entity.ToTable("Pago");

            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Nombre).HasMaxLength(60);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK_Payments_9B556A386F8C5028");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");


        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK_rol_2A49584C755F3A90");

            entity.ToTable("rol");

            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_usuario_3214EC07331E0F3E");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.Correo, "UQ_usuario_2A586E0B9B066502").IsUnique();

			entity.Property(e => e.Celular)
				.HasMaxLength(10)
				.IsUnicode(false)
				.HasColumnName("celular");
			entity.Property(e => e.Cedula)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("cedula");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre_usuario");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK_usuarioIdRol_3A81B327");
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.IdAuto).HasName("PK_Vehiculo_B191F5E656E4EEC9");

            entity.Property(e => e.IdAuto).HasColumnName("id_auto");
            entity.Property(e => e.Año).HasColumnName("año");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("color");
            entity.Property(e => e.Marca)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("marca");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Imagen).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.ImagenId)
                .HasConstraintName("FK_Imagen");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}