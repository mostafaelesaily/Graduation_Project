import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

class HomeGreeting extends StatelessWidget {
  final String userName;
  final String location;
  final VoidCallback? onLocationTap;

  const HomeGreeting({
    super.key,
    required this.userName,
    this.location = 'Downtown Boston',
    this.onLocationTap,
  });

  String get _greeting {
    final hour = DateTime.now().hour;
    if (hour < 12) return 'Good morning,';
    if (hour < 17) return 'Good afternoon,';
    return 'Good evening,';
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'YOUR HEALTH JOURNEY',
          style: TextStyle(
            color: ColorsManager.primaryColor,
            fontSize: 12.sp,
            fontWeight: FontWeight.bold,
            letterSpacing: 0.8,
          ),
        ),
        SizedBox(height: 8.h),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Text(
                '$_greeting\n$userName',
                style: TextStyle(
                  color: ColorsManager.textDark,
                  fontSize: 28.sp,
                  fontWeight: FontWeight.bold,
                  height: 1.2,
                ),
              ),
            ),
            Text('👋', style: TextStyle(fontSize: 24.sp)),
            SizedBox(width: 8.w),
            _LocationChip(label: location, onTap: onLocationTap),
          ],
        ),
      ],
    );
  }
}

class _LocationChip extends StatelessWidget {
  final String label;
  final VoidCallback? onTap;

  const _LocationChip({required this.label, this.onTap});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.white,
      shape: StadiumBorder(side: BorderSide(color: ColorsManager.cardBorder)),
      child: InkWell(
        customBorder: const StadiumBorder(),
        onTap: onTap,
        child: Padding(
          padding: EdgeInsets.symmetric(horizontal: 12.w, vertical: 9.h),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                Icons.location_on,
                color: ColorsManager.primaryColor,
                size: 16.sp,
              ),
              SizedBox(width: 4.w),
              ConstrainedBox(
                constraints: BoxConstraints(maxWidth: 88.w),
                child: Text(
                  label,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: ColorsManager.textDark,
                    fontSize: 13.sp,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
              Icon(
                Icons.keyboard_arrow_down_rounded,
                color: ColorsManager.fadedText,
                size: 18.sp,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
