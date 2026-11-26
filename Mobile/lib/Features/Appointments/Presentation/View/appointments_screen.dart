import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_cubit.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_state.dart';
import 'package:medlink/Features/Appointments/Presentation/Widgets/appointment_card.dart';

class AppointmentsScreen extends StatelessWidget {
  const new({super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.only(left: 20.h, right: 20.h),
      child: Column(
        spacing: 16.h,
        children: [_Title(), _Filters(), _Appointments()],
      ),
    );
  }
}

class _Title extends StatelessWidget {
  const new({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: .stretch,
      children: [
        Text('Your Care Schedule', style: TextStyles.authTitle),
        Text(
          'Track, prepare for, and manage your medical consultations',
          style: TextStyles.fadedText,
        ),
      ],
    );
  }
}

class _Filters extends StatelessWidget {
  const new({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocSelector<AppointmentsCubit, AppointmentsState, int>(
      selector: (state) => state.chosenFilter,
      builder: (context, chosenFilter) {
        return Container(
          padding: EdgeInsets.all(4),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16),
            border: BoxBorder.all(color: ColorsManager.cardBorder),
          ),
          child: Row(
            spacing: 4,
            mainAxisAlignment: .spaceAround,
            children: [
              _FiltersButton(
                title: 'Upcoming',
                isChosen: chosenFilter == 0,
                onPressed: () {
                  context.read<AppointmentsCubit>().changeFilter(0);
                },
              ),
              _FiltersButton(
                title: 'Past Visits',
                isChosen: chosenFilter == 1,
                onPressed: () {
                  context.read<AppointmentsCubit>().changeFilter(1);
                },
              ),
              _FiltersButton(
                title: 'Cancelled',
                isChosen: chosenFilter == 2,
                onPressed: () {
                  context.read<AppointmentsCubit>().changeFilter(2);
                },
              ),
            ],
          ),
        );
      },
    );
  }
}

class _Appointments extends StatelessWidget {
  const new();

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: ListView.separated(
        itemBuilder: (context, index) {
          return AppointmentCard(
            doctorName: 'Abdallah',
            specialty: 'Programmer',
            date: '1/1/2028',
            location: 'Cairo',
            imageUrl: '',
            onViewDetails: () {},
          );
        },
        separatorBuilder: (context, index) => SizedBox(height: 16.h),
        itemCount: 2,
      ),
    );
  }
}

class _FiltersButton extends StatelessWidget {
  final bool isChosen;
  final String title;
  final void Function()? onPressed;
  const new({
    this.isChosen = false,
    required this.title,
    required this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: TextButton(
        onPressed: onPressed,
        style: TextButton.styleFrom(
          backgroundColor: isChosen ? Color(0xffEAF4FF) : null,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadiusGeometry.circular(12),
          ),
          maximumSize: Size(double.infinity, 37.5),
          minimumSize: Size(double.infinity, 37.5),
          padding: EdgeInsets.zero,
          tapTargetSize: .shrinkWrap,
          visualDensity: .standard,
        ),

        child: Text(
          title,
          style: TextStyle(
            color: !isChosen ? Color(0xff6B8299) : ColorsManager.primaryColor,
            fontSize: 13,
            fontWeight: .w600,
          ),
        ),
      ),
    );
  }
}
