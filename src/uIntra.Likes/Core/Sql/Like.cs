﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uIntra.Core.Persistence;

namespace uIntra.Likes
{
    [uIntraTable("Like")]
    public class Like : SqlEntity<Guid>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override Guid Id { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Index("UQ_Like_UserId_EntityId", 1, IsUnique = true)]
        public Guid UserId { get; set; }

        [Required]
        [Index("UQ_Like_UserId_EntityId", 2, IsUnique = true)]
        public Guid EntityId { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}