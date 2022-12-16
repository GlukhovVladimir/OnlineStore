﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationTechSale.HelperServices;

namespace WebApplicationTechSale.Models
{
    public class EditItemViewModel
    {
        [Required]
        public string Id { get; set; }

        [Display(Name = "Название товара")]
        [Required(ErrorMessage = "Укажите название товара")]
        [MaxLength(100, ErrorMessage = "Не более 100 символов")]
        public string Name { get; set; }

        [Required]
        public string OldName { get; set; }

        [Display(Name = "Фотография")]
        [Required(ErrorMessage = "Загрузите фотографию товара")]
        [DataType(DataType.Upload)]
        [ExtensionValidation(new string[]
        { ".jpg", ".jpeg", ".pjpg", ".pjpeg", ".png" }, ErrorMessage = "Неверный формат файла")]
        public IFormFile Photo { get; set; }

        [Required]
        public string OldPhotoSrc { get; set; }

        [Display(Name = "Описание товара")]
        [Required(ErrorMessage = "Добавьте описание товара")]
        [MaxLength(500, ErrorMessage = "Не более 500 символов")]
        public string Description { get; set; }

        [Display(Name = "Цена")]
        [Required(ErrorMessage = "Укажите цену")]
        [Range(0, 1000000, ErrorMessage = "Цена не должна быть меньше нуля")]
        public int? Price { get; set; }
    }
}