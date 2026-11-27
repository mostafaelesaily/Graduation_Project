import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Core/Dialogs/error_dialog.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/Cubit/provider_profile_cubit.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/Cubit/provider_profile_state.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/widgets/location_section.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/widgets/profile_header.dart';

class ProviderProfileScreen extends StatelessWidget {
  final ProviderProfile profile;

  const ProviderProfileScreen({super.key, required this.profile});

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<ProviderProfileCubit, ProviderProfileState>(
      listener: (context, state) {
        if (state.status == BookingStatus.success) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text(
                'Booking request sent. Waiting for provider approval.',
              ),
            ),
          );
        }

        if (state.status == BookingStatus.failure) {
          showDialog(
            context: context,
            builder: (context) {
              return ErrorDialog(
                error: state.errorMessage?['message'].toString(),
              );
            },
          );
        }
      },
      builder: (context, state) {
        return Scaffold(
          backgroundColor: ColorsManager.primaryBackgroundColor,
          appBar: AppBar(
            backgroundColor: Colors.white,
            surfaceTintColor: Colors.white,
            leading: IconButton(
              icon: const Icon(Icons.arrow_back_ios_new),
              onPressed: () =>
                  context.canPop() ? context.pop() : context.go('/home'),
            ),
            title: const Text(
              'Provider Profile',
              style: TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
          body: ListView(
            padding: EdgeInsets.all(16.w),
            children: [
              ProfileHeader(profile: profile),
              LocationSection(address: profile.address),
            ],
          ),
          bottomNavigationBar: _BookingBar(
            providerId: profile.id,
            isLoading: state.status == BookingStatus.loading,
          ),
        );
      },
    );
  }
}

class _BookingBar extends StatelessWidget {
  final String providerId;
  final bool isLoading;

  const _BookingBar({required this.providerId, required this.isLoading});

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.white,
      child: SafeArea(
        top: false,
        child: Padding(
          padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 10.h),
          child: AuthButton(
            onPressed: isLoading ? null : () => _book(context),
            text: isLoading ? null : 'Book Visit',
            icon: isLoading ? const CircularProgressIndicator() : null,
          ),
        ),
      ),
    );
  }

  // The backend only answers "is this time available?", so the patient
  // picks a date and time and the cubit checks it before booking.
  Future<void> _book(BuildContext context) async {
    final cubit = context.read<ProviderProfileCubit>();
    final now = DateTime.now();

    final date = await showDatePicker(
      context: context,
      initialDate: now,
      firstDate: now,
      lastDate: now.add(const Duration(days: 60)),
    );
    if (date == null || !context.mounted) return;

    final time = await showTimePicker(
      context: context,
      initialTime: TimeOfDay.fromDateTime(now.add(const Duration(hours: 1))),
    );
    if (time == null) return;

    await cubit.book(
      providerId: providerId,
      slot: DateTime(date.year, date.month, date.day, time.hour, time.minute),
    );
  }
}
