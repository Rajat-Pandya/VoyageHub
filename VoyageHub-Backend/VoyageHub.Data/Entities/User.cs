using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoyageHub.Data.Entities;

[Table("USERS")]
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Employee"; // Employee, Manager, Finance, Admin

    public Guid? DepartmentId { get; set; }
    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    public Guid? ManagerId { get; set; }
    [ForeignKey(nameof(ManagerId))]
    public User? Manager { get; set; }

    public ICollection<User> Subordinates { get; set; } = new List<User>();

    [MaxLength(255)]
    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpires { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TravelRequest> TravelRequests { get; set; } = new List<TravelRequest>();
    public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();
}

