import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Home/Presentation/Models/home_models.dart';

class UpcomingCareSection extends StatelessWidget {
  final List<UpcomingAppointment> appointments;
  final VoidCallback? onViewAll;

  const UpcomingCareSection({
    super.key,
    required this.appointments,
    this.onViewAll,
  });

  @override
  Widget build(BuildContext context) {
    if (appointments.isEmpty) return const SizedBox.shrink();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Text(
              'Upcoming Care',
              style: TextStyle(
                color: ColorsManager.textDark,
                fontSize: 20.sp,
                fontWeight: FontWeight.bold,
              ),
            ),
            SizedBox(width: 8.w),
            Container(
              width: 8.w,
              height: 8.w,
              decoration: const BoxDecoration(
                color: ColorsManager.primaryColor,
                shape: BoxShape.circle,
              ),
            ),
            const Spacer(),
            GestureDetector(
              onTap: onViewAll,
              child: Text(
                'View All (${appointments.length})',
                style: TextStyle(
                  color: ColorsManager.primaryColor,
                  fontSize: 14.sp,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          ],
        ),
        SizedBox(height: 12.h),
        _AppointmentCard(appointment: appointments.first),
      ],
    );
  }
}

class _AppointmentCard extends StatelessWidget {
  final UpcomingAppointment appointment;

  const _AppointmentCard({required this.appointment});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.all(14.w),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(22.r),
        border: Border.all(color: ColorsManager.cardBorder),
        boxShadow: [
          BoxShadow(
            color: ColorsManager.primaryColor.withValues(alpha: 0.05),
            blurRadius: 20,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Row(
        children: [
          Stack(
            clipBehavior: Clip.none,
            children: [
              CircleAvatar(
                radius: 32.r,
                backgroundColor: ColorsManager.softBlue,
                child: Icon(
                  Icons.person,
                  color: ColorsManager.primaryColor,
                  size: 34.sp,
                ),
              ),
              if (appointment.isVideo)
                Positioned(
                  right: -2.w,
                  bottom: -2.w,
                  child: Container(
                    padding: EdgeInsets.all(5.w),
                    decoration: BoxDecoration(
                      color: ColorsManager.primaryColor,
                      shape: BoxShape.circle,
                      border: Border.all(color: Colors.white, width: 2),
                    ),
                    child: Icon(
                      Icons.videocam_rounded,
                      color: Colors.white,
                      size: 12.sp,
                    ),
                  ),
                ),
            ],
          ),
          SizedBox(width: 14.w),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  appointment.doctorName,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: ColorsManager.textDark,
                    fontSize: 16.sp,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                SizedBox(height: 4.h),
                Text(
                  '${appointment.specialty} • ${appointment.hospital}',
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: ColorsManager.fadedText,
                    fontSize: 13.sp,
                  ),
                ),
              ],
            ),
          ),
          SizedBox(width: 8.w),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Container(
                padding: EdgeInsets.symmetric(horizontal: 10.w, vertical: 4.h),
                decoration: BoxDecoration(
                  color: ColorsManager.softBlue,
                  borderRadius: BorderRadius.circular(20.r),
                  border: Border.all(color: ColorsManager.cardBorder),
                ),
                child: Text(
                  appointment.dayLabel,
                  style: TextStyle(
                    color: ColorsManager.primaryColor,
                    fontSize: 12.sp,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
              SizedBox(height: 10.h),
              Text(
                appointment.timeLabel,
                style: TextStyle(
                  color: ColorsManager.textDark,
                  fontSize: 16.sp,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
