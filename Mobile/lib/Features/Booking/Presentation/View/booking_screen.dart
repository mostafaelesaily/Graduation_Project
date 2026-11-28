import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Core/Dialogs/error_dialog.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Features/Booking/Presentation/Cubit/booking_cubit.dart';
import 'package:medlink/Features/Booking/Presentation/Cubit/booking_state.dart';
import 'package:medlink/Features/Booking/Presentation/View/widgets/date_labels.dart';
import 'package:medlink/Features/Booking/Presentation/View/widgets/day_picker.dart';
import 'package:medlink/Features/Booking/Presentation/View/widgets/time_slots.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/widgets/section_card.dart';

class BookingScreen extends StatefulWidget {
  final ProviderProfile profile;

  const BookingScreen({super.key, required this.profile});

  @override
  State<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends State<BookingScreen> {
  final _notes = TextEditingController();

  @override
  void dispose() {
    _notes.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<BookingCubit, BookingState>(
      listenWhen: (previous, current) => previous.status != current.status,
      listener: (context, state) {
        if (state.status == BookingStatus.success) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(
                state.message ??
                    'Booking request sent. Waiting for provider approval.',
              ),
            ),
          );
          context.pop();
        }

        if (state.status == BookingStatus.failure) {
          showDialog(
            context: context,
            builder: (context) => ErrorDialog(error: state.message),
          );
        }
      },
      builder: (context, state) {
        final cubit = context.read<BookingCubit>();

        return Scaffold(
          backgroundColor: ColorsManager.primaryBackgroundColor,
          appBar: AppBar(
            backgroundColor: Colors.white,
            surfaceTintColor: Colors.white,
            leading: IconButton(
              icon: const Icon(Icons.arrow_back_ios_new),
              onPressed: () => context.pop(),
            ),
            title: const Text(
              'Appointment Booking',
              style: TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
          body: ListView(
            padding: EdgeInsets.all(16.w),
            children: [
              _buildProvider(),
              SizedBox(height: 16.h),
              _buildTitle('Consultation Mode'),
              _buildMode(),
              SizedBox(height: 16.h),
              DayPicker(
                selectedDay: state.selectedDay,
                onSelected: cubit.selectDay,
              ),
              _buildSlots(state, cubit),
              SizedBox(height: 20.h),
              _buildTitle('Notes for the provider (Optional)'),
              TextField(
                controller: _notes,
                maxLength: 1000,
                minLines: 3,
                maxLines: 5,
                decoration: InputDecoration(
                  hintText: 'Briefly describe your reason for the visit...',
                  hintStyle: TextStyles.fadedText,
                  filled: true,
                  fillColor: Colors.white,
                  border: OutlineInputBorder(
                    borderSide: BorderSide.none,
                    borderRadius: BorderRadius.circular(16.r),
                  ),
                ),
              ),
            ],
          ),
          bottomNavigationBar: _buildBottomBar(state, cubit),
        );
      },
    );
  }

  Widget _buildTitle(String title) {
    return Padding(
      padding: EdgeInsets.only(bottom: 10.h),
      child: Text(
        title,
        style: TextStyle(
          color: ColorsManager.textDark,
          fontSize: 16.sp,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Widget _buildProvider() {
    return SectionCard(
      title: widget.profile.serviceName,
      child: _buildRow(
        Icons.location_on_outlined,
        widget.profile.address.isEmpty
            ? 'No address listed.'
            : widget.profile.address,
      ),
    );
  }

  Widget _buildMode() {
    return Container(
      padding: EdgeInsets.all(16.w),
      decoration: BoxDecoration(
        color: ColorsManager.softBlue,
        borderRadius: BorderRadius.circular(16.r),
      ),
      child: _buildRow(Icons.apartment, 'In-Person Clinic Visit'),
    );
  }

  Widget _buildRow(IconData icon, String text) {
    return Row(
      spacing: 10.w,
      children: [
        Icon(icon, color: ColorsManager.primaryColor, size: 22.sp),
        Expanded(
          child: Text(
            text,
            style: TextStyle(color: ColorsManager.textDark, fontSize: 14.sp),
          ),
        ),
      ],
    );
  }

  Widget _buildSlots(BookingState state, BookingCubit cubit) {
    if (state.slotsStatus == SlotsStatus.loading) {
      return Padding(
        padding: EdgeInsets.all(24.w),
        child: const Center(child: CircularProgressIndicator()),
      );
    }

    if (state.slotsStatus == SlotsStatus.failure) {
      return Column(
        children: [
          SizedBox(height: 16.h),
          const Text('Could not load the times.', style: TextStyles.fadedText),
          TextButton(onPressed: cubit.loadSlots, child: const Text('Retry')),
        ],
      );
    }

    if (state.slots.isEmpty) {
      return Padding(
        padding: EdgeInsets.all(24.w),
        child: const Text(
          'No available times on this day.',
          textAlign: TextAlign.center,
          style: TextStyles.fadedText,
        ),
      );
    }

    return TimeSlots(
      slots: state.slots,
      selectedSlot: state.selectedSlot,
      onSelected: cubit.selectSlot,
    );
  }

  Widget _buildBottomBar(BookingState state, BookingCubit cubit) {
    final slot = state.selectedSlot;
    final isLoading = state.status == BookingStatus.loading;

    return Container(
      color: Colors.white,
      child: SafeArea(
        top: false,
        child: Padding(
          padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 10.h),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            spacing: 10.h,
            children: [
              _buildRow(
                Icons.calendar_today_outlined,
                slot == null ? 'Choose a time' : dateTimeLabel(slot),
              ),
              AuthButton(
                onPressed: slot == null || isLoading
                    ? null
                    : () => cubit.confirm(_notes.text.trim()),
                text: isLoading ? null : 'Confirm & Schedule Appointment →',
                icon: isLoading ? const CircularProgressIndicator() : null,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
