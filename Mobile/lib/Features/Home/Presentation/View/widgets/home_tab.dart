import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Features/Home/Presentation/Models/home_models.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/ai_navigator_card.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/care_services_grid.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_greeting.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_header.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/top_providers_section.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/upcoming_care_section.dart';

class HomeTab extends StatelessWidget {
  final String userName;

  const HomeTab({super.key, required this.userName});

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: EdgeInsets.fromLTRB(20.w, 12.h, 20.w, 24.h),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const HomeHeader(hasUnreadNotifications: true),
          SizedBox(height: 20.h),
          HomeGreeting(userName: userName),
          SizedBox(height: 20.h),
          const AiNavigatorCard(),
          SizedBox(height: 24.h),
          const UpcomingCareSection(appointments: HomeMockData.appointments),
          SizedBox(height: 24.h),
          const CareServicesGrid(services: HomeMockData.services),
          SizedBox(height: 24.h),
          const TopProvidersSection(providers: HomeMockData.providers),
        ],
      ),
    );
  }
}
