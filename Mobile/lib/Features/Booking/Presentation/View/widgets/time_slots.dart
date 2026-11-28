import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Booking/Presentation/Cubit/booking_state.dart';
import 'package:medlink/Features/Booking/Presentation/View/widgets/date_labels.dart';

/// The times of one day, grouped into morning, afternoon and evening.
class TimeSlots extends StatelessWidget {
  final List<TimeSlot> slots;
  final DateTime? selectedSlot;
  final ValueChanged<DateTime> onSelected;

  const TimeSlots({
    super.key,
    required this.slots,
    required this.selectedSlot,
    required this.onSelected,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        ..._buildGroup(
          'MORNING',
          Icons.wb_twilight,
          slots.where((slot) => slot.time.hour < 12),
        ),
        ..._buildGroup(
          'AFTERNOON',
          Icons.wb_sunny_outlined,
          slots.where((slot) => slot.time.hour >= 12 && slot.time.hour < 17),
        ),
        ..._buildGroup(
          'EVENING',
          Icons.nights_stay_outlined,
          slots.where((slot) => slot.time.hour >= 17),
        ),
      ],
    );
  }

  List<Widget> _buildGroup(
    String title,
    IconData icon,
    Iterable<TimeSlot> group,
  ) {
    if (group.isEmpty) return const [];

    return [
      Padding(
        padding: EdgeInsets.symmetric(vertical: 10.h),
        child: Row(
          spacing: 6.w,
          children: [
            Icon(icon, size: 18.sp, color: ColorsManager.fadedText),
            Text(
              title,
              style: TextStyle(
                color: ColorsManager.fadedText,
                fontSize: 12.sp,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
      ),
      LayoutBuilder(
        builder: (context, constraints) {
          // Three buttons in each row.
          final width = (constraints.maxWidth - 16.w) / 3;

          return Wrap(
            spacing: 8.w,
            runSpacing: 8.h,
            children: [
              for (final slot in group)
                SizedBox(width: width, child: _buildSlot(slot)),
            ],
          );
        },
      ),
    ];
  }

  Widget _buildSlot(TimeSlot slot) {
    final isSelected = slot.time == selectedSlot;
    final color = isSelected
        ? Colors.white
        : slot.isAvailable
        ? ColorsManager.textDark
        : ColorsManager.fadedText;

    return Material(
      color: isSelected
          ? ColorsManager.primaryColor
          : slot.isAvailable
          ? Colors.white
          : ColorsManager.softBlue,
      borderRadius: BorderRadius.circular(14.r),
      child: InkWell(
        borderRadius: BorderRadius.circular(14.r),
        onTap: slot.isAvailable ? () => onSelected(slot.time) : null,
        child: Padding(
          padding: EdgeInsets.symmetric(vertical: 12.h),
          child: Text(
            timeLabel(slot.time),
            textAlign: TextAlign.center,
            style: TextStyle(
              color: color,
              fontSize: 13.sp,
              fontWeight: FontWeight.w600,
              decoration: slot.isAvailable ? null : TextDecoration.lineThrough,
            ),
          ),
        ),
      ),
    );
  }
}
