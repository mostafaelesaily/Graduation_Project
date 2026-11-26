import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

class HomeHeader extends StatelessWidget {
  final VoidCallback? onNotificationsTap;
  final VoidCallback? onProfileTap;
  final bool hasUnreadNotifications;

  const HomeHeader({
    super.key,
    this.onNotificationsTap,
    this.onProfileTap,
    this.hasUnreadNotifications = false,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Image.asset('assets/logo/logo.png', height: 44.h, fit: BoxFit.contain),
        const Spacer(),
        _CircleButton(
          icon: Icons.notifications_none_rounded,
          background: Colors.white,
          iconColor: ColorsManager.textDark,
          borderColor: ColorsManager.cardBorder,
          showDot: hasUnreadNotifications,
          onTap: onNotificationsTap,
        ),
        SizedBox(width: 12.w),
        _CircleButton(
          icon: Icons.person_outline_rounded,
          background: ColorsManager.primaryColor,
          iconColor: Colors.white,
          onTap: onProfileTap,
        ),
      ],
    );
  }
}

class _CircleButton extends StatelessWidget {
  final IconData icon;
  final Color background;
  final Color iconColor;
  final Color? borderColor;
  final bool showDot;
  final VoidCallback? onTap;

  const _CircleButton({
    required this.icon,
    required this.background,
    required this.iconColor,
    this.borderColor,
    this.showDot = false,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Material(
      color: background,
      shape: CircleBorder(
        side: borderColor == null
            ? BorderSide.none
            : BorderSide(color: borderColor!),
      ),
      child: InkWell(
        customBorder: const CircleBorder(),
        onTap: onTap,
        child: SizedBox(
          width: 44.w,
          height: 44.w,
          child: Stack(
            alignment: Alignment.center,
            children: [
              Icon(icon, color: iconColor, size: 22.sp),
              if (showDot)
                Positioned(
                  top: 11.w,
                  right: 12.w,
                  child: Container(
                    width: 8.w,
                    height: 8.w,
                    decoration: const BoxDecoration(
                      color: Colors.redAccent,
                      shape: BoxShape.circle,
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }
}
