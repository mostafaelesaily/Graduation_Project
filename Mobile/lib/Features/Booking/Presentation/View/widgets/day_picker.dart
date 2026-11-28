import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Booking/Presentation/View/widgets/date_labels.dart';

/// Five days at a time, from today and up to 60 days ahead.
class DayPicker extends StatefulWidget {
  final DateTime selectedDay;
  final ValueChanged<DateTime> onSelected;

  const DayPicker({
    super.key,
    required this.selectedDay,
    required this.onSelected,
  });

  @override
  State<DayPicker> createState() => _DayPickerState();
}

class _DayPickerState extends State<DayPicker> {
  static const _pageSize = 5;
  static const _lastPage = 11;

  int _page = 0;

  @override
  Widget build(BuildContext context) {
    final now = DateTime.now();
    final days = [
      for (var i = 0; i < _pageSize; i++)
        DateTime(now.year, now.month, now.day + _page * _pageSize + i),
    ];

    return Column(
      spacing: 10.h,
      children: [
        Row(
          children: [
            Expanded(
              child: Text(
                'Choose Date',
                style: TextStyle(
                  color: ColorsManager.textDark,
                  fontSize: 16.sp,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
            IconButton(
              onPressed: _page == 0 ? null : () => setState(() => _page--),
              icon: const Icon(Icons.chevron_left),
            ),
            Text(
              '${monthNames[days.first.month - 1]} ${days.first.year}',
              style: TextStyle(
                color: ColorsManager.textDark,
                fontSize: 13.sp,
                fontWeight: FontWeight.bold,
              ),
            ),
            IconButton(
              onPressed: _page == _lastPage
                  ? null
                  : () => setState(() => _page++),
              icon: const Icon(Icons.chevron_right),
            ),
          ],
        ),
        Row(
          spacing: 8.w,
          children: [for (final day in days) Expanded(child: _buildDay(day))],
        ),
      ],
    );
  }

  Widget _buildDay(DateTime day) {
    final isSelected = DateUtils.isSameDay(day, widget.selectedDay);

    return Material(
      color: isSelected ? ColorsManager.primaryColor : Colors.white,
      borderRadius: BorderRadius.circular(16.r),
      child: InkWell(
        borderRadius: BorderRadius.circular(16.r),
        onTap: () => widget.onSelected(day),
        child: Padding(
          padding: EdgeInsets.symmetric(vertical: 12.h),
          child: Column(
            spacing: 4.h,
            children: [
              Text(
                weekdayNames[day.weekday - 1],
                style: TextStyle(
                  color: isSelected ? Colors.white : ColorsManager.fadedText,
                  fontSize: 12.sp,
                ),
              ),
              Text(
                '${day.day}',
                style: TextStyle(
                  color: isSelected ? Colors.white : ColorsManager.textDark,
                  fontSize: 18.sp,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
