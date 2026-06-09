using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Text;

namespace QL_CFE_WPF.ViewModels
{
    public class NhomHangTreeVM : ObservableObject
    {
        public int Id { get; set; }

        public string TenNhom { get; set; }
        public int? ParentId { get; set; }

        public ObservableCollection<NhomHangTreeVM> Children { get; set; }
            = new();
        public bool IsRoot => ParentId == null;

        public string IconKind
        {
            get
            {
                return TenNhom switch
                {
                    "Cafe" => "Coffee",
                    "Trà" => "Tea",
                    "Bia" => "Beer",
                    "Sữa" => "BottleSoda",
                    "Thịt" => "FoodSteak",
                    "Cá" => "Fish",
                    "Hải sản" => "Shrimp",
                    "Gà" => "FoodDrumstick",
                    "Pizza" => "Pizza",
                    "Hamburger" => "Hamburger",
                    "Bánh" => "CakeVariant",
                    "Kho" => "Warehouse",
                    _ => "FolderOutline"
                };
            }
        }
        //màu Icon
        public Brush IconColor
        {
            get
            {
                return TenNhom switch
                {
                    "Cafe" => Brushes.SaddleBrown,
                    "Trà" => Brushes.ForestGreen,
                    "Bia" => Brushes.Goldenrod,
                    "Sữa" => Brushes.SteelBlue,
                    "Thịt" => Brushes.Firebrick,
                    "Cá" => Brushes.DodgerBlue,
                    "Hải sản" => Brushes.Teal,
                    "Bánh" => Brushes.HotPink,
                    _ => Brushes.Gray
                };
            }
        }
    }
}
