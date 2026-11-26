import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Home/Presentation/Models/home_models.dart';

class CareServicesGrid extends StatelessWidget {
  final List<CareService> services;
  final ValueChanged<CareService>? onServiceTap;

  const CareServicesGrid({super.key, required this.services, this.onServiceTap});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Explore Care Services',
          style: TextStyle(
            color: ColorsManager.textDark,
            fontSize: 20.sp,
            fontWeight: FontWeight.bold,
          ),
        ),
        SizedBox(height: 12.h),
        GridView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: services.length,
          gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 3,
            mainAxisSpacing: 12.h,
            crossAxisSpacing: 12.w,
            childAspectRatio: 0.88,
          ),
          itemBuilder: (context, index) {
            final service = services[index];
            return _ServiceTile(
              service: service,
              onTap: () => onServiceTap?.call(service),
            );
          },
        ),
      ],
    );
  }
}

class _ServiceTile extends StatelessWidget {
  final CareService service;
  final VoidCallback onTap;

  const _ServiceTile({required this.service, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.white,
      borderRadius: BorderRadius.circular(20.r),
      child: InkWell(
        borderRadius: BorderRadius.circular(20.r),
        onTap: onTap,
        child: Ink(
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(20.r),
            border: Border.all(color: ColorsManager.cardBorder),
          ),
          child: Padding(
            padding: EdgeInsets.symmetric(horizontal: 6.w, vertical: 10.h),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  width: 46.w,
                  height: 46.w,
                  decoration: BoxDecoration(
                    color: ColorsManager.softBlue,
                    borderRadius: BorderRadius.circular(14.r),
                  ),
                  child: Icon(
                    service.icon,
                    color: ColorsManager.primaryColor,
                    size: 24.sp,
                  ),
                ),
                SizedBox(height: 10.h),
                Text(
                  service.label,
                  textAlign: TextAlign.center,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: ColorsManager.textDark,
                    fontSize: 13.sp,
                    fontWeight: FontWeight.w600,
                    height: 1.25,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
