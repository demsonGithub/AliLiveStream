   M S B A M L   `   `   `  ����D  @LiveSteam, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:LiveSteam	LiveSteam  5   	       LiveSteam.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:LiveSteam     5   	     � Title$  推流��5   	   $	��450��6   $
��1205��6!   .��5	      �  � RowDefinitions 5
   
   �� 5      $	��100 �6   6+   �� 5      6   �� 5      $	��100 �6   6+    � ColumnDefinitions 5   
   �� 5      6!   �� 5      6!   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��0��6.   $��2��6;   .+�5   
   �� $��操作消息:��6   $��30(�6"   $��Center=�60   �� 5      -   5         ��Name$ txtMsg��5      $	��无��6&   $��20(�61   $��Center=�6?   $��20 8 0 0q�6Z   5      �� 5   
   $ 
Horizontal=�6   $��1��6.   .+�5      �� -   5         ��Name$ 	cmbCamera��5      $	��200��6*   $��50��66   $��	20 0 30 0q�6B   5      �� 5      -   5         ��Name$ btnBegin��5      $	��150��6(   $��80��64   $��开启推流��6@   6h   5      �� 5   
   -   5        $ btnOver��5      $��1��6"   $��1��60   $	��150��6@   $��80��6L   $��结束推流��6X   6   �� 5    
   -   5         ��Name$ txtResponse��5       $��2��6)   $��2��66   $	��Msg��6J   $��30(�6U   $��Center=�6c   5"      5#      