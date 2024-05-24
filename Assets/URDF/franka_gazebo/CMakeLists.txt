cmake_minimum_required(VERSION 3.0.2)
project(franka_gazebo)

#set(CMAKE_CXX_STANDARD 17)

find_package(Boost REQUIRED COMPONENTS thread)

find_package(
    catkin REQUIRED
    COMPONENTS
    roscpp
    roslib
    xacro
    gazebo_dev
    gazebo_ros
    gazebo_plugins
    geometry_msgs
    )

catkin_package(
    INCLUDE_DIRS 
        include
    LIBRARIES 
        franka_gazebo
    CATKIN_DEPENDS
        roscpp
        xacro
        gazebo_dev
        gazebo_ros
        gazebo_plugins
        geometry_msgs
)

###########
## Build ##
###########

## Specify additional locations of header files
## Your package locations should be listed before other locations
include_directories(
    include
    ${catkin_INCLUDE_DIRS}
)

## Declare a C++ library
add_library(${PROJECT_NAME} src/franka_gazebo.cpp)
target_link_libraries(${PROJECT_NAME} ${catkin_LIBRARIES})

add_library(franka_handle src/franka_handle.cpp)
target_link_libraries(franka_handle ${catkin_LIBRARIES} ${Boost_LIBRARIES})

## Mark executables for installation
## See http://docs.ros.org/melodic/api/catkin/html/howto/format1/building_executables.html

## Mark libraries for installation
## See http://docs.ros.org/melodic/api/catkin/html/howto/format1/building_libraries.html
install(TARGETS ${PROJECT_NAME}
  ARCHIVE DESTINATION ${CATKIN_PACKAGE_LIB_DESTINATION}
  LIBRARY DESTINATION ${CATKIN_PACKAGE_LIB_DESTINATION}
  RUNTIME DESTINATION ${CATKIN_GLOBAL_BIN_DESTINATION}
)

## Mark cpp header files for installation
install(
    DIRECTORY include/${PROJECT_NAME}/
    DESTINATION ${CATKIN_PACKAGE_INCLUDE_DESTINATION}
)
